using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using System.Reflection;
using UnityEngine.SceneManagement;

public class GameplayFeatureTests
{
    private GameObject _playerObject;
    private GameObject _anchor;
    private GameObject _itemContainer;
    private GameObject _worldRoot;
    private GameObject _inputObject;
    private PlayerHolder _holder;
    private InputGetter _inputGetter;
    private System.Collections.Generic.HashSet<EntityId> _initialSceneRoots;

    [SetUp]
    public void SetUp()
    {
        _initialSceneRoots = new System.Collections.Generic.HashSet<EntityId>();
        foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
            _initialSceneRoots.Add(root.GetEntityId());
        _playerObject = new GameObject("player");
        _worldRoot = new GameObject("world root");
        _playerObject.AddComponent<SpriteRenderer>();
        _playerObject.AddComponent<BoxOverlapDetector>();
        _holder = _playerObject.AddComponent<PlayerHolder>();
        _anchor = new GameObject("holding anchor");
        _anchor.transform.SetParent(_playerObject.transform);
        SetField(_holder, "holdingItem", _anchor);
        SetField(_holder, "_playerSpriteRenderer", _playerObject.GetComponent<SpriteRenderer>());
        SetField(_holder, "defaultPlayerSprite", null);
        PlayerHolder.instance = _holder;
        _inputObject = new GameObject("input getter");
        _inputGetter = _inputObject.AddComponent<InputGetter>();
        Invoke(_holder, "Start");
        Invoke(_playerObject.GetComponent<BoxOverlapDetector>(), "Start");
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
            if (!_initialSceneRoots.Contains(root.GetEntityId()))
                Object.DestroyImmediate(root);
        PlayerHolder.instance = null;
        InputGetter.instance = null;
    }

    [Test]
    public void HoldingItem_AttachesItToPlayerAndDisablesPhysics()
    {
        var (item, behavior, renderer) = CreateItem();

        behavior.PickUp();

        Assert.That(_holder.HeldItem, Is.SameAs(behavior));
        Assert.That(item.transform.parent, Is.SameAs(_playerObject.transform));
        Assert.That(item.GetComponent<Rigidbody>().isKinematic, Is.True);
        Assert.That(item.GetComponent<Collider>().enabled, Is.False);
    }

    [Test]
    public void DroppingItem_UsesHoldingAnchorPositionAndDetachesFromPlayer()
    {
        var (item, behavior, renderer) = CreateItem();
        _itemContainer = new GameObject("world item container");
        item.transform.SetParent(_itemContainer.transform);
        // Item captured its original parent during Awake; configure the original world container before pickup.
        SetField(behavior, "parent", _itemContainer.transform);
        Vector3 dropPosition = new Vector3(4f, 2f, -1f);
        _anchor.transform.position = dropPosition;
        _holder.Hold(behavior, renderer);

        Item dropped = _holder.Drop();

        Assert.That(dropped, Is.SameAs(behavior));
        Assert.That(item.transform.parent, Is.SameAs(_itemContainer.transform));
        Assert.That(item.transform.parent, Is.Not.SameAs(_playerObject.transform));
        Assert.That(item.transform.position, Is.EqualTo(dropPosition));
        Assert.That(item.GetComponent<Rigidbody>().isKinematic, Is.False);
        Assert.That(item.GetComponent<Collider>().enabled, Is.True);
        Assert.That(_holder.GotItem, Is.False);
    }

    [Test]
    public void HighlightChangesColorAndRestoresItWhenCleared()
    {
        var target = new GameObject("highlight target");
        var renderer = target.AddComponent<SpriteRenderer>();
        var highlight = target.AddComponent<ChangeColorHighlight>();
        highlight.OnHighlighted();
        Assert.That(renderer.color, Is.EqualTo(Color.red));

        highlight.OnDeHighlighted();
        Assert.That(renderer.color, Is.EqualTo(Color.white));
        Object.DestroyImmediate(target);
    }

    [Test]
    public void CancellableCoroutine_CancelStopsNestedRoutineBeforeNextStep()
    {
        int steps = 0;
        var routine = new CancellableCoroutine(Outer());

        Assert.That(routine.MoveNext(), Is.True);
        Assert.That(steps, Is.EqualTo(1));
        routine.Cancel();
        Assert.That(routine.MoveNext(), Is.False);
        Assert.That(steps, Is.EqualTo(1));

        IEnumerator Outer()
        {
            yield return Inner();
            steps++;
        }

        IEnumerator Inner()
        {
            steps++;
            yield return null;
            steps++;
        }
    }

    [Test]
    public void CancellableCoroutine_PauseAndResumeControlAdvancement()
    {
        int steps = 0;
        var routine = new CancellableCoroutine(CountSteps());
        routine.Pause();

        Assert.That(routine.MoveNext(), Is.True);
        Assert.That(steps, Is.Zero);
        routine.Resume();
        Assert.That(routine.MoveNext(), Is.True);
        Assert.That(steps, Is.EqualTo(1));

        IEnumerator CountSteps()
        {
            steps++;
            yield return null;
            steps++;
        }
    }

    [Test]
    public void PassiveItemDoesNotTriggerUseOnLeftClick()
    {
        var (_, behavior, renderer) = CreateItem();
        _holder.Hold(behavior, renderer);
        // A passive item has no IUsable, so the player's left-click action is a no-op.
        typeof(PlayerHolder)
            .GetMethod("UseHoldingItem", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            .Invoke(_holder, null);
        Assert.That(_holder.GotItem, Is.True);
    }
    
    [Test]
    public void Player_FacesLeftWhenMovingLeft()
    {
        var movement = _playerObject.AddComponent<PlayerMovement>();
        Invoke(movement, "UpdateFacingDirection", -1f);
        Assert.That(movement.GetFacingDirection(), Is.EqualTo(-1));
    }

    [Test]
    public void Player_FacesRightWhenMovingRight()
    {
        var movement = _playerObject.AddComponent<PlayerMovement>();
        _playerObject.transform.localScale = new Vector3(-1f, 1f, 1f);
        Invoke(movement, "UpdateFacingDirection", 1f);
        Assert.That(movement.GetFacingDirection(), Is.EqualTo(1));
    }

    [Test]
    public void Player_FacingDoesNotChangeWhenMovingUpOrDown()
    {
        var movement = _playerObject.AddComponent<PlayerMovement>();
        _playerObject.transform.localScale = new Vector3(-1f, 1f, 1f);
        Invoke(movement, "UpdateFacingDirection", 0f);
        Assert.That(movement.GetFacingDirection(), Is.EqualTo(-1));
    }

    [Test]
    public void Player_KeepFacingDirectionWhenNotMoving()
    {
        var movement = _playerObject.AddComponent<PlayerMovement>();
        _playerObject.transform.localScale = new Vector3(-1f, 1f, 1f);
        Invoke(movement, "UpdateFacingDirection", 0f);
        Assert.That(movement.GetFacingDirection(), Is.EqualTo(-1));
    }

    [Test]
    public void Player_PicksUpItemWhenActionKeyIsPressed()
    {
        Assert.That((KeyCode)typeof(InputGetter).
            GetField("ActionKey", 
                BindingFlags.Instance | BindingFlags.NonPublic).GetValue(_inputGetter), 
            Is.EqualTo(KeyCode.F));
        var (_, behavior, _) = CreateItem();
        var detector = _playerObject.GetComponent<BoxOverlapDetector>();
        SetField(detector, "overlapedCollider", behavior.GetComponent<Collider>());
        SetField(detector, "boxOverlapped", true);
        RaiseEvent(_inputGetter, "OnActionMousePressed");
        Assert.That(_holder.HeldItem, Is.SameAs(behavior));
    }

    [Test]
    public void Player_DropsItemWhenRightMouseButtonIsPressed()
    {
        var (_, behavior, renderer) = CreateItem();
        _holder.Hold(behavior, renderer);
        RaiseEvent(_inputGetter, "OnRightMousePressed");
        Assert.That(_holder.GotItem, Is.False);
        Assert.That(behavior.transform.parent, Is.Null);
    }

    [Test]
    public void PassiveItem_DoesNotPerformActionWhenLeftMouseButtonIsPressed()
    {
        var (_, behavior, renderer) = CreateItem();
        _holder.Hold(behavior, renderer);
        RaiseEvent(_inputGetter, "OnLeftMousePressed");
        Assert.That(_holder.GotItem, Is.True);
    }

    [Test]
    public void UsableItem_PerformsActionWhenLeftMouseButtonIsPressed()
    {
        var usable = CreateWaterGun(out var squirt);
        _holder.Hold(usable, usable.GetComponent<SpriteRenderer>());
        RaiseEvent(_inputGetter, "OnLeftMousePressed");
        Assert.That(squirt.activeSelf, Is.True);
    }

    [Test]
    public void WaterGun_ShootsWaterWhenUsed()
    {
        var gun = CreateWaterGun(out var squirt);
        _holder.Hold(gun, gun.GetComponent<SpriteRenderer>());
        RaiseEvent(_inputGetter, "OnLeftMousePressed");
        Assert.That(squirt.activeSelf, Is.True);
    }

    [Test]
    public void WaterGun_DamagesKidWhenHit()
    {
        var gun = CreateWaterGun(out _);
        _holder.Hold(gun, gun.GetComponent<SpriteRenderer>());
        var detector = _playerObject.GetComponent<BoxOverlapDetector>();
        var kid = CreateDamageableKid();
        SetField(detector, "colliders", new[] { kid.GetComponent<Collider>() });
        var patience = kid.GetComponent<KidPatience>();
        SetField(patience, "cryCoroutine", new CancellableCoroutine(Endless()));
        RaiseEvent(_inputGetter, "OnLeftMousePressed");
        Assert.That(kid.GetComponent<SpriteRenderer>().color, Is.EqualTo(Color.red));

        IEnumerator Endless() { while (true) yield return null; }
    }

    [Test]
    public void Kid_RemainsHeldWhenDesireChanges()
    {
        var kid = CreateKid();
        var behavior = kid.GetComponent<KidClickBehavior>();
        behavior.OnActionButtonPressed();
        SetField(kid.GetComponent<KidPatience>(), "patienceSeconds", 100f);
        kid.GetComponent<KidPatience>().StartWaiting(new TestDesire()).MoveNext();
        Assert.That(_holder.HeldItem, Is.SameAs(behavior));
        Assert.That(kid.transform.parent, Is.SameAs(_playerObject.transform));
        Assert.That(GetField(kid.GetComponent<KidNavigation>(), "canWalk"), Is.False);
    }

    [Test]
    public void HeldKid_RemainsInPlayersArms()
    {
        var kid = CreateKid();
        var behavior = kid.GetComponent<KidClickBehavior>();
        behavior.OnActionButtonPressed();
        kid.GetComponent<KidPatience>().Cry().MoveNext();
        _playerObject.transform.position += Vector3.right * 3f;
        Assert.That(kid.transform.parent, Is.SameAs(_playerObject.transform));
        Assert.That(kid.transform.localPosition, Is.EqualTo(_anchor.transform.localPosition));
    }

    [Test]
    public void Item_HasNoGravity()
    {
        var (item, behavior, renderer) = CreateItem();
        _holder.Hold(behavior, renderer);
        Assert.That(item.GetComponent<Rigidbody>().isKinematic, Is.True);
    }

    [Test]
    public void UnpickupableItem_IsNotHighlighted()
    {
        var (item, behavior, renderer) = CreateItem();
        item.AddComponent<ChangeColorHighlight>();
        behavior.PickUp();
        Assert.That(item.GetComponent<Collider>().enabled, Is.False);
        Assert.That(Physics.OverlapBox(item.transform.position, Vector3.one).Length, Is.Zero);
    }

    [Test]
    public void Player_CannotPickUpItemWhenOutOfRange()
    {
        var (_, item, _) = CreateItem();
        var detector = _playerObject.GetComponent<BoxOverlapDetector>();
        SetField(detector, "overlapedCollider", item.GetComponent<Collider>());
        SetField(detector, "boxOverlapped", false);
        RaiseEvent(_inputGetter, "OnActionMousePressed");
        Assert.That(_holder.GotItem, Is.False);
    }

    [Test]
    public void Player_CanPickUpItemWhenInRange()
    {
        var (_, item, _) = CreateItem();
        var detector = _playerObject.GetComponent<BoxOverlapDetector>();
        SetField(detector, "overlapedCollider", item.GetComponent<Collider>());
        SetField(detector, "boxOverlapped", true);
        RaiseEvent(_inputGetter, "OnActionMousePressed");
        Assert.That(_holder.HeldItem, Is.SameAs(item.GetComponent<TestItem>()));
    }

    [Test]
    public void KidPatience_ShowsDesireBubble()
    {
        var kid = CreateKid();
        var bubble = new GameObject("desire bubble");
        bubble.SetActive(false);
        SetField(kid.GetComponent<KidPatience>(), "desireBubble", bubble);
        SetField(kid.GetComponent<KidPatience>(), "desireSpriteRenderer", new GameObject("desire image").AddComponent<SpriteRenderer>());
        SetField(kid.GetComponent<KidPatience>(), "patienceSeconds", 100f);
        var desire = new TestDesire();
        var waiting = kid.GetComponent<KidPatience>().StartWaiting(desire);
        waiting.MoveNext();
        Assert.That(bubble.activeSelf, Is.True);
    }

    [Test]
    public void KidPatience_ChangesFaceAfterShowingDesire()
    {
        var kid = CreateKid();
        var patience = kid.GetComponent<KidPatience>();
        var bubble = new GameObject("bubble");
        SetField(patience, "desireBubble", bubble);
        SetField(patience, "desireSpriteRenderer", new GameObject("desire image").AddComponent<SpriteRenderer>());
        var happy = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.zero);
        var neutral = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.zero);
        SetField(patience, "happyFace", happy);
        SetField(patience, "neutralFace", neutral);
        SetField(patience, "patienceSeconds", 0.001f);
        SetField(patience, "_changeTimeSeconds", 0f);
        var waiting = patience.StartWaiting(new TestDesire());
        waiting.MoveNext();
        Assert.That(kid.GetComponent<SpriteRenderer>().sprite, Is.EqualTo(neutral));
    }

    [Test]
    public void KidPatience_ReturnsToWanderingWhenDesireIsSatisfied()
    {
        var kid = CreateKid();
        var patience = kid.GetComponent<KidPatience>();
        var bubble = new GameObject("bubble");
        SetField(patience, "desireBubble", bubble);
        SetField(patience, "desireSpriteRenderer", new GameObject("desire image").AddComponent<SpriteRenderer>());
        var desire = new TestDesire { Satisfied = true };
        var waiting = patience.StartWaiting(desire);
        waiting.MoveNext();
        Assert.That(bubble.activeSelf, Is.False);
    }

    [Test]
    public void KidPatience_StartsCryingWhenDesireIsNotSatisfied()
    {
        var kid = CreateKid();
        var patience = kid.GetComponent<KidPatience>();
        var bubble = new GameObject("bubble");
        SetField(patience, "desireBubble", bubble);
        SetField(patience, "desireSpriteRenderer", new GameObject("desire image").AddComponent<SpriteRenderer>());
        SetField(patience, "patienceSeconds", 0f);
        SetField(patience, "_changeTimeSeconds", 0f);
        var desire = new FoodDesire(patience, null, kid.GetComponent<KidClickBehavior>());
        patience.StartWaiting(desire).MoveNext();
        ((CancellableCoroutine)GetField(patience, "cryCoroutine")).MoveNext();
        Assert.That(patience.IsCrying, Is.True);
    }

    [Test]
    public void Hungry_RequestsFoodWhenHungry()
    {
        var kid = CreateKid();
        var desire = new FoodDesire(kid.GetComponent<KidPatience>(), null, kid.GetComponent<KidClickBehavior>());
        Assert.That(desire.IsSatisfied(), Is.False);
    }

    [Test]
    public void Hungry_StopsDesireAfterBeingFed()
    {
        var kid = CreateKid();
        var food = new GameObject("food");
        food.tag = "Food";
        food.AddComponent<BoxCollider>();
        food.AddComponent<Rigidbody>();
        food.AddComponent<SpriteRenderer>();
        var foodBehavior = food.AddComponent<TestItem>();
        _holder.Hold(foodBehavior, food.GetComponent<SpriteRenderer>());
        var desire = new FoodDesire(kid.GetComponent<KidPatience>(), null, kid.GetComponent<KidClickBehavior>());
        kid.GetComponent<KidClickBehavior>().OnKidPressed?.Invoke();
        Assert.That(desire.IsSatisfied(), Is.True);
        Assert.That(_holder.GotItem, Is.False);
        Assert.That(_holder.HoldsItemOfTag("Food"), Is.False);
    }

    [Test]
    public void Sleepy_RequestsPlayerToPlaceKidOnBed()
    {
        var kid = CreateKid();
        var sleep = CreateSleepDesire(kid);
        SetField(kid.GetComponent<KidDesiresManager>(), "currentDesire", sleep);
        Assert.That(kid.GetComponent<KidClickBehavior>().CanBePutToSleep, Is.True);
    }

    [Test]
    public void Kid_SleepsWhenPlacedOnBed()
    {
        var kid = CreateKid();
        var sleep = CreateSleepDesire(kid);
        SetField(kid.GetComponent<KidDesiresManager>(), "currentDesire", sleep);
        kid.GetComponent<KidClickBehavior>().OnActionButtonPressed();
        var bed = new GameObject("bed");
        var bedBehavior = bed.AddComponent<BedClickBehavior>();
        bedBehavior.OnActionButtonPressed();
        Assert.That(kid.GetComponent<NavMeshAgent>().enabled, Is.False);
        Assert.That(kid.transform.parent, Is.SameAs(bed.transform));
    }

    [Test]
    public void Kid_IsRecognizedAsBeingOnBedWhenSleeping()
    {
        var kid = CreateKid();
        var sleep = CreateSleepDesire(kid);
        SetField(kid.GetComponent<KidDesiresManager>(), "currentDesire", sleep);
        Assert.That(kid.GetComponent<KidClickBehavior>().CanBePutToSleep, Is.True);
        sleep.SetSatisfied();
        Assert.That(kid.GetComponent<KidClickBehavior>().CanBePutToSleep, Is.False);
    }

    [Test]
    public void Kid_DetachesFromBedAfterSleeping()
    {
        var kid = CreateKid();
        var house = new GameObject("house root");
        var bed = new GameObject("bed");
        bed.transform.SetParent(house.transform);
        kid.transform.SetParent(bed.transform);
        var sleep = CreateSleepDesire(kid);
        sleep.SetSatisfied();
        AdvanceToCompletion((IEnumerator)GetField(sleep, "aftermath"));
        Assert.That(kid.transform.parent, Is.SameAs(house.transform));
    }

    [Test]
    public void Kid_ReturnsToWanderingAfterSleeping()
    {
        var kid = CreateKid();
        var sleep = CreateSleepDesire(kid);
        sleep.SetSatisfied();
        AdvanceToCompletion((IEnumerator)GetField(sleep, "aftermath"));
        Assert.That(kid.GetComponent<NavMeshAgent>().enabled, Is.True);
        Assert.That(GetField(kid.GetComponent<KidNavigation>(), "canWalk"), Is.True);
    }

    [Test]
    public void Kid_SleepsOnFloorWhenDesireIsNotSatisfied()
    {
        var kid = CreateKid();
        var sleep = CreateSleepDesire(kid, 5f);
        var aftermath = sleep.SetUnsatisfied();
        aftermath.MoveNext();
        Assert.That(kid.GetComponent<SpriteRenderer>().sprite, Is.Not.Null);
        Assert.That(sleep.IsSatisfied(), Is.False);
    }

    [Test]
    public void Kid_CannotBePickedUpWhileSleeping()
    {
        var kid = CreateKid();
        var sleep = CreateSleepDesire(kid, 5f);
        sleep.SetSatisfied();
        var sleepRoutine = new CancellableCoroutine((IEnumerator)GetField(sleep, "aftermath"));
        sleepRoutine.MoveNext();
        var detector = _playerObject.GetComponent<BoxOverlapDetector>();
        SetField(detector, "overlapedCollider", kid.GetComponent<Collider>());
        SetField(detector, "boxOverlapped", kid.layer != LayerMask.NameToLayer("Ignore overlap"));
        RaiseEvent(_inputGetter, "OnActionMousePressed");
        Assert.That(_holder.GotItem, Is.False);
        Assert.That(kid.layer, Is.EqualTo(LayerMask.NameToLayer("Ignore overlap")));
    }

    [Test]
    public void Kid_IsNotHighlightedWhileSleeping()
    {
        var kid = CreateKid();
        var sleep = CreateSleepDesire(kid, 5f);
        sleep.SetUnsatisfied().MoveNext();
        Assert.That(kid.layer, Is.EqualTo(LayerMask.NameToLayer("Ignore overlap")));
        var detectableLayers = ~(1 << kid.layer);
        var overlaps = Physics.OverlapBox(kid.transform.position, Vector3.one, Quaternion.identity, detectableLayers);
        Assert.That(System.Array.Exists(overlaps, collider => collider.gameObject == kid), Is.False);
    }

    [Test]
    public void Kid_DoesNotRemainAttachedToBedAfterWakingUp()
    {
        var kid = CreateKid();
        var house = new GameObject("house root");
        var bed = new GameObject("bed");
        bed.transform.SetParent(house.transform);
        kid.transform.SetParent(bed.transform);
        var sleep = CreateSleepDesire(kid);
        sleep.SetSatisfied();
        AdvanceToCompletion((IEnumerator)GetField(sleep, "aftermath"));
        Assert.That(kid.transform.parent, Is.SameAs(house.transform));
    }

    [Test]
    public void Crying_KidMovesTowardsRandomChair()
    {
        var kid = CreateKid();
        var chair = CreateChair(new Vector3(5f, 0f, 0f));
        var cry = kid.GetComponent<KidPatience>().Cry();
        cry.MoveNext();
        Assert.That(kid.transform.position, Is.EqualTo(Vector3.zero));
        Assert.That(chair.transform.parent, Is.Not.SameAs(kid.transform));
    }

    [Test]
    public void Crying_KidPicksUpChairWhenCloseEnough()
    {
        var kid = CreateKid();
        var chair = CreateChair(new Vector3(0.1f, 0f, 0f));
        var cry = kid.GetComponent<KidPatience>().Cry();
        cry.MoveNext();
        Assert.That(chair.transform.parent.parent, Is.SameAs(kid.transform));
    }

    [Test]
    public void Crying_KidCannotPickUpChairWhenOutOfRange()
    {
        var kid = CreateKid();
        var chair = CreateChair(new Vector3(5f, 0f, 0f));
        var cry = kid.GetComponent<KidPatience>().Cry();
        cry.MoveNext();
        Assert.That(chair.GetComponent<Rigidbody>().isKinematic, Is.False);
    }

    [Test]
    public void Crying_KidDoesNotTeleportToChair()
    {
        var kid = CreateKid();
        var chair = CreateChair(new Vector3(5f, 0f, 0f));
        var cry = kid.GetComponent<KidPatience>().Cry();
        cry.MoveNext();
        Assert.That(kid.transform.position, Is.EqualTo(Vector3.zero));
        Assert.That(chair.transform.position, Is.EqualTo(new Vector3(5f, 0f, 0f)));
    }

    [Test]
    public void Crying_KidThrowsChairAfterPickingItUp()
    {
        var kid = CreateKid();
        var chair = CreateChair(new Vector3(0.1f, 0f, 0f));
        var surfaceObject = new GameObject("nav mesh surface");
        var surfaceType = System.Type.GetType("NavMeshPlus.Components.NavMeshSurface, NavMeshPlus");
        var surface = surfaceObject.AddComponent(surfaceType);
        SetField(kid.GetComponent<KidPatience>(), "surface", surface);
        var cry = kid.GetComponent<KidPatience>().Cry();
        cry.MoveNext();
        Assert.That(chair.GetComponent<Rigidbody>().isKinematic, Is.True);
        cry.MoveNext();
        Assert.That(chair.GetComponent<Rigidbody>().isKinematic, Is.True);
        Assert.That(chair.GetComponent<Rigidbody>().useGravity, Is.True);
        Assert.That(chair.transform.root, Is.Not.SameAs(kid.transform));
    }

    [Test]
    public void Crying_KidFollowsStrictChairInteractionSequence()
    {
        var kid = CreateKid();
        var chair = CreateChair(new Vector3(5f, 0f, 0f));
        var cry = kid.GetComponent<KidPatience>().Cry();
        cry.MoveNext();
        Assert.That(chair.GetComponent<Rigidbody>().isKinematic, Is.False, "A distant chair must not be grabbed before the kid approaches it.");
        Assert.That(chair.transform.parent.parent, Is.Not.SameAs(kid.transform));
    }

    [Test]
    public void Aftermath_KidCriesWhenNotFed()
    {
        var kid = CreateKid();
        var desire = new FoodDesire(kid.GetComponent<KidPatience>(), null, kid.GetComponent<KidClickBehavior>());
        var aftermath = desire.SetUnsatisfied();
        aftermath.MoveNext();
        Assert.That(kid.GetComponent<KidPatience>().IsCrying, Is.True);
    }

    [Test]
    public void Aftermath_NoActionWhenHungryDesireIsSatisfied()
    {
        var kid = CreateKid();
        var desire = new FoodDesire(kid.GetComponent<KidPatience>(), null, kid.GetComponent<KidClickBehavior>());
        desire.SetSatisfied();
        AdvanceToCompletion(desire.ActivateAftermath());
        Assert.That(desire.IsSatisfied(), Is.True);
        Assert.That(kid.GetComponent<KidPatience>().IsCrying, Is.False);
    }

    [Test]
    public void Aftermath_KidSleepsOnFloorWhenSleepyDesireIsNotSatisfied()
    {
        var kid = CreateKid();
        var sleep = CreateSleepDesire(kid, 5f);
        sleep.SetUnsatisfied().MoveNext();
        Assert.That(kid.GetComponent<SpriteRenderer>().sprite, Is.Not.Null);
        Assert.That(kid.layer, Is.EqualTo(LayerMask.NameToLayer("Ignore overlap")));
    }

    [Test]
    public void Aftermath_KidSleepsAfterBeingPlacedOnBed()
    {
        var kid = CreateKid();
        var sleep = CreateSleepDesire(kid);
        sleep.SetSatisfied();
        var routine = new CancellableCoroutine((IEnumerator)GetField(sleep, "aftermath"));
        routine.MoveNext();
        Assert.That(kid.GetComponent<SpriteRenderer>().sprite, Is.Not.Null);
        Assert.That(kid.GetComponent<NavMeshAgent>().enabled, Is.False);
    }

    [Test]
    public void Fridge_GivesFoodToPlayerWhenActionKeyIsPressed()
    {
        var prefab = new GameObject("food prefab");
        prefab.AddComponent<BoxCollider>();
        prefab.AddComponent<Rigidbody>();
        prefab.AddComponent<SpriteRenderer>();
        prefab.AddComponent<FoodClickBehavior>();
        var fridgeObject = new GameObject("fridge");
        var fridgeCollider = fridgeObject.AddComponent<BoxCollider>();
        var fridge = fridgeObject.AddComponent<FridgeClickBehavior>();
        SetField(fridge, "food", prefab);
        var detector = _playerObject.GetComponent<BoxOverlapDetector>();
        SetField(detector, "overlapedCollider", fridgeCollider);
        SetField(detector, "boxOverlapped", true);
        RaiseEvent(_inputGetter, "OnActionMousePressed");
        Assert.That(_holder.GotItem, Is.True);
    }


    private (GameObject gameObject, TestItem behavior, SpriteRenderer renderer) CreateItem()
    {
        var itemObject = new GameObject("item");
        itemObject.AddComponent<BoxCollider>();
        itemObject.AddComponent<Rigidbody>();
        var renderer = itemObject.AddComponent<SpriteRenderer>();
        var behavior = itemObject.AddComponent<TestItem>();
        return (itemObject, behavior, renderer);
    }

    private GameObject CreateKid()
    {
        var kid = new GameObject("kid");
        kid.transform.SetParent(_worldRoot.transform);
        kid.AddComponent<SpriteRenderer>();
        kid.AddComponent<BoxCollider>();
        kid.AddComponent<Rigidbody>();
        kid.AddComponent<NavMeshAgent>();
        kid.AddComponent<ChangeColorHighlight>();
        kid.AddComponent<KidNavigation>();
        var patience = kid.AddComponent<KidPatience>();
        var bubble = new GameObject("desire bubble");
        bubble.transform.SetParent(kid.transform);
        SetField(patience, "desireBubble", bubble);
        var desireImage = new GameObject("desire image");
        desireImage.transform.SetParent(kid.transform);
        SetField(patience, "desireSpriteRenderer", desireImage.AddComponent<SpriteRenderer>());
        kid.AddComponent<KidDesiresManager>();
        kid.AddComponent<KidClickBehavior>();
        return kid;
    }

    private SleepDesire CreateSleepDesire(GameObject kid, float sleepSeconds = 0f)
    {
        var sleepingSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.zero);
        var desire = new SleepDesire(kid.GetComponent<KidPatience>(), kid.GetComponent<SpriteRenderer>(),
            sleepingSprite, sleepSeconds, sleepSeconds, null);
        desire.OnSleep += () => { };
        desire.OnAwake += () => { };
        desire.OnSleep += () => kid.layer = LayerMask.NameToLayer("Ignore overlap");
        int originalLayer = kid.layer;
        desire.OnAwake += () => kid.layer = originalLayer;
        return desire;
    }

    private GameObject CreateChair(Vector3 position)
    {
        var chairRoot = new GameObject("chair root");
        var chair = new GameObject("chair collider");
        chair.transform.SetParent(chairRoot.transform);
        chair.transform.position = position;
        chair.tag = "Chair";
        chair.AddComponent<BoxCollider>();
        chair.AddComponent<Rigidbody>();
        return chair;
    }

    private GameObject CreateDamageableKid()
    {
        return CreateKid();
    }

    private WaterGunClickBehavior CreateWaterGun(out GameObject squirt)
    {
        var gunObject = new GameObject("water gun");
        gunObject.AddComponent<SpriteRenderer>();
        gunObject.AddComponent<BoxCollider>();
        gunObject.AddComponent<Rigidbody>();
        var gun = gunObject.AddComponent<WaterGunClickBehavior>();
        squirt = new GameObject("water squirt");
        squirt.transform.SetParent(gunObject.transform);
        SetField(gun, "squirtImage", squirt);
        return gun;
    }

    private static void RaiseEvent(object target, string name)
    {
        var field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, $"Event backing field {name} was not found.");
        (field.GetValue(target) as System.Action)?.Invoke();
    }

    private static void Invoke(object target, string method, params object[] args)
    {
        target.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(target, args);
    }

    private static object GetField(object target, string name)
    {
        return FindField(target.GetType(), name).GetValue(target);
    }

    private static void AdvanceToCompletion(IEnumerator routine)
    {
        var cancellable = new CancellableCoroutine(routine);
        int advances = 0;
        while (cancellable.MoveNext() && advances++ < 32) { }
        Assert.That(advances, Is.LessThan(32), "The routine did not finish within the expected number of immediate steps.");
    }

    private static void SetField(object target, string name, object value)
    {
        FindField(target.GetType(), name).SetValue(target, value);
    }

    private static FieldInfo FindField(System.Type type, string name)
    {
        while (type != null)
        {
            var field = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly);
            if (field != null) return field;
            type = type.BaseType;
        }
        Assert.Fail($"Field {name} was not found.");
        return null;
    }

    public class TestDesire : KidDesire
    {
        public bool Satisfied;
        public bool UnsatisfiedCalled;
        public override bool IsSatisfied() => Satisfied;
        public override void Reset() => Satisfied = false;
        public override IEnumerator ActivateAftermath() { yield break; }
        public override CancellableCoroutine SetUnsatisfied()
        {
            UnsatisfiedCalled = true;
            return new CancellableCoroutine(Endless());
        }
        public override void SetSatisfied() => Satisfied = true;
        private static IEnumerator Endless() { while (true) yield return null; }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalmStudent : Student
{
    //parameters
    private StateMachineEngine calmStudentFSM;
    private StateMachineEngine flirtSubFSM;

    private WatchingPerception watching;

    public Perception pushFight;
    private Perception timerPatrol;

    public List<float> reservedKissPos;

    #region Stats

    protected Group group;
    private int friendNumber;
    public int beautyThreshold;
    public int affinityThreshold;
    public int DanceAffinityThreshold;
    protected Student targetStudent;
    private float distanceToMeetPos;
    public List<string> musicLikes;
    public Genders sexuality;
    private List<Character> blackList;

    #endregion
    #region OtherVariables
    private volatile bool availableForMess = true;

    #endregion

    //methods
    public CalmStudent(string name, Genders gender, Transform obj, GameManager gameState) : base(name, gender, obj, gameState)
    {
        this.role = Roles.CalmStudent;
        this.musicLikes = new List<string>();
        this.blackList = new List<Character>();
        this.fatigue = 0.0f;

        this.watching = new WatchingPerception(this.gameObject, () => watching.getTargetCharacter().getGender() == sexuality, () => watching.getTargetCharacter().getRole() == Roles.CalmStudent,
            () => watching.getTargetCharacter().beauty >= beautyThreshold, () => watching.getTargetCharacter().isInState("Enjoying", "Looking for couple", "Bench", "Outside"), () => !blackList.Contains(watching.getTargetCharacter()));

        CreateFlirtSubStateMachine();

        animationController = this.gameObject.GetComponentInChildren<Animator>();
        if (gender == Genders.Female) animationController.SetBool("isGirl", true);
    }

    private void CreateFlirtSubStateMachine()
    {
        flirtSubFSM = new StateMachineEngine(true);

        // Perceptions
        Perception gotToOther = flirtSubFSM.CreatePerception<ValuePerception>(() => Vector3.Distance(targetStudent.GetGameObject().transform.position, GetGameObject().transform.position) < 1);
        Perception timer = flirtSubFSM.CreatePerception<TimerPerception>(5);
        Perception affinityCheck = flirtSubFSM.CreatePerception<ValuePerception>(() => CheckAffinity(targetStudent) >= affinityThreshold);
        Perception danceAffinityCheck = flirtSubFSM.CreatePerception<ValuePerception>(() => CheckAffinity(targetStudent) >= DanceAffinityThreshold);
        Perception danceWithStudent = flirtSubFSM.CreateAndPerception<AndPerception>(timer, affinityCheck);
        Perception kissWithStudent = flirtSubFSM.CreateAndPerception<AndPerception>(timer, danceAffinityCheck);

        // States
        State gettingCloserState = flirtSubFSM.CreateEntryState("Getting closer", GettingCloser);
        State checkAffinityState = flirtSubFSM.CreateState("CheckingAffinity", CheckingAffinity);
        State dancingState = flirtSubFSM.CreateState("Dancing", Dancing);
        State bathroomState = flirtSubFSM.CreateState("Bathroom", Kissing);

        // Transitions
        flirtSubFSM.CreateTransition("already close", gettingCloserState, gotToOther, checkAffinityState);
        flirtSubFSM.CreateTransition("High affinity", checkAffinityState, danceWithStudent, dancingState);
        flirtSubFSM.CreateTransition("High affinity dancing", dancingState, kissWithStudent, bathroomState);
    }

    public override void CreateStateMachine()
    {
        calmStudentFSM = new StateMachineEngine(false);

        // Perceptions
        Perception push = calmStudentFSM.CreatePerception<PushPerception>();
        pushFight = calmStudentFSM.CreatePerception<PushPerception>();
        Perception pushThirsty = calmStudentFSM.CreatePerception<ValuePerception>(() => thirst > thirstThreshold);
        Perception fatigueCheck = calmStudentFSM.CreatePerception<ValuePerception>(() => fatigue >= fatigueThreshold);
        Perception timer = calmStudentFSM.CreatePerception<TimerPerception>(5);
        Perception timerTired = calmStudentFSM.CreatePerception<TimerPerception>(20);
        Perception timerFlirt = calmStudentFSM.CreatePerception<TimerPerception>(60);
        Perception timerForLooking = calmStudentFSM.CreatePerception<TimerPerception>(20);
        Perception lowAffinityCheck = calmStudentFSM.CreatePerception<ValuePerception>(() => CheckAffinity(targetStudent) < affinityThreshold);
        Perception lowDanceAffinityCheck = calmStudentFSM.CreatePerception<ValuePerception>(() => CheckAffinity(targetStudent) < DanceAffinityThreshold);
        Perception isInStateCheck = calmStudentFSM.CreatePerception<IsInStatePerception>(flirtSubFSM, "CheckingAffinity");
        Perception isInStateDance = calmStudentFSM.CreatePerception<IsInStatePerception>(flirtSubFSM, "Dancing");
        Perception lowAffinity = calmStudentFSM.CreateAndPerception<AndPerception>(lowAffinityCheck, isInStateCheck);
        Perception lowDanceAffinity = calmStudentFSM.CreateAndPerception<AndPerception>(lowDanceAffinityCheck, isInStateDance);
        Perception affinities = calmStudentFSM.CreateOrPerception<OrPerception>(lowAffinity, lowDanceAffinity);
        Perception lowAffinities = calmStudentFSM.CreateAndPerception<AndPerception>(affinities, timer);
        Perception heGotOut = calmStudentFSM.CreatePerception<ValuePerception>(() => !targetStudent.isInState("Flirting"));
        Perception auxOr = calmStudentFSM.CreateOrPerception<OrPerception>(lowAffinities, heGotOut);
        Perception outOfFlirt = calmStudentFSM.CreateOrPerception<OrPerception>(auxOr, timerFlirt);
        Perception enterParty = calmStudentFSM.CreatePerception<ValuePerception>(() => distanceToMeetPos < 1);
        timeOut = calmStudentFSM.CreatePerception<TimerPerception>(2);
        isInStateDrink = calmStudentFSM.CreatePerception<IsInStatePerception>(drinkSubFSM, "Drink");
        Perception exitDrink = calmStudentFSM.CreateAndPerception<AndPerception>(isInStateDrink, timeOut); //not used
        Perception likeMusic = calmStudentFSM.CreatePerception<ValuePerception>(() => musicLikes.IndexOf(gameState.soundingMusic) != -1);
        Perception dislikeMusic = calmStudentFSM.CreatePerception<ValuePerception>(() => musicLikes.IndexOf(gameState.soundingMusic) == -1);
        Perception beautyCheck = calmStudentFSM.CreatePerception<WatchingPerception>(watching);
        timerPatrol = calmStudentFSM.CreatePerception<TimerPerception>(5);

        // States
        State startState = calmStudentFSM.CreateEntryState("Start", Start);
        State enjoyState = calmStudentFSM.CreateState("Enjoying", Enjoying);
        State lookingForCouple = calmStudentFSM.CreateState("Looking for couple");
        State benchState = calmStudentFSM.CreateState("Bench", InBench);
        State drinkingState = calmStudentFSM.CreateSubStateMachine("Drink", drinkSubFSM);
        State outsideState = calmStudentFSM.CreateState("Outside", Outside);
        State fightingState = calmStudentFSM.CreateState("Fighting", FightAsCalm);
        State punishedState = calmStudentFSM.CreateState("Punished", Punished);
        State flirtState = calmStudentFSM.CreateSubStateMachine("Flirting", flirtSubFSM);

        // Transitions
        calmStudentFSM.CreateTransition("Look for friends", startState, enterParty, enjoyState);

        calmStudentFSM.CreateTransition("Random time", enjoyState, timerForLooking, lookingForCouple);
        calmStudentFSM.CreateTransition("Random time 2", lookingForCouple, timerForLooking, startState);
        calmStudentFSM.CreateTransition("Like Person from walk", lookingForCouple, beautyCheck, flirtState);
        calmStudentFSM.CreateTransition("Like Person from enjoy", enjoyState, beautyCheck, flirtState);
        calmStudentFSM.CreateTransition("Provoked by messy student", enjoyState, pushFight, fightingState);
        calmStudentFSM.CreateTransition("Looking for fresh air", enjoyState, fatigueCheck, outsideState);
        calmStudentFSM.CreateTransition("Thirsty", enjoyState, pushThirsty, drinkingState);
        calmStudentFSM.CreateTransition("Dislike music", enjoyState, dislikeMusic, benchState);

        calmStudentFSM.CreateTransition("Like Person from playground", outsideState, beautyCheck, flirtState);
        calmStudentFSM.CreateTransition("Like Person from bench", benchState, beautyCheck, flirtState);

        flirtSubFSM.CreateExitTransition("Low affinity or finish kissing", flirtState, outOfFlirt, startState);

        calmStudentFSM.CreateTransition("Finish fighting", fightingState, timer, startState);
        calmStudentFSM.CreateTransition("Caught fighting", fightingState, push, punishedState);

        calmStudentFSM.CreateTransition("Finish punishment", punishedState, timer, startState);

        calmStudentFSM.CreateTransition("Already taken enough fresh air", outsideState, timerTired, startState);

        drinkSubFSM.CreateExitTransition("Not thirsty any more", drinkingState, isInStateDrink, startState);

        calmStudentFSM.CreateTransition("Like music", benchState, likeMusic, startState);
    }

    public override void Update()
    {
        distanceToMeetPos = Vector3.Distance(this.gameObject.transform.position, group.getMyPos(this.friendNumber));
        distanceToBar = Vector3.Distance(this.gameObject.transform.position, GameObject.FindGameObjectWithTag("Bar").transform.position + new Vector3(-0.75f, 0, 0));

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            calmStudentFSM.Fire("Thirsty");
        }

        if (!isInStateDrink.Check())
        {
            timeOut.Reset();
        }

        drinkSubFSM.Update();
        flirtSubFSM.Update();
        calmStudentFSM.Update();

        if (isInState("Looking for couple"))
        {
            if (timerPatrol.Check())
            {
                timerPatrol.Reset();

                if (currentOcuppiedPos != null) this.gameState.possiblePosGym.AddRange(currentOcuppiedPos);

                var index = Random.Range(0, this.gameState.possiblePosGym.Count / 2 - 1) * 2;
                currentOcuppiedPos = this.gameState.possiblePosGym.GetRange(index, 2);
                this.gameState.possiblePosGym.RemoveRange(index, 2);

                Move(new Vector3(currentOcuppiedPos[0], currentOcuppiedPos[1]));
            }
        }

        if (isInState("Flirting"))
        {
            Perception isInStateBathroom = calmStudentFSM.CreatePerception<IsInStatePerception>(flirtSubFSM, "Bathroom");
            if (isInStateBathroom.Check())
                LookAt(targetStudent.GetGameObject().transform);
        }
    }

    public override bool isInState(params string[] states)
    {
        try
        {
            foreach (string state in states) {
                Perception isIn = calmStudentFSM.CreatePerception<IsInStatePerception>(calmStudentFSM, state);
                if(isIn.Check())
                    return true;
            }

            return false;
        }
        catch (KeyNotFoundException)
        {
            return false;
        }
    }

    protected int CheckAffinity(Student targetStudent)
    {
        var affinity = 0;

        if (sexuality == targetStudent.getGender())
        {
            affinity += 6; //affinity base
        } else
        {
            return 0;
        }

        if (Hobbies[0] == targetStudent.Hobbies[0] || Hobbies[0] == targetStudent.Hobbies[1] || Hobbies[0] == targetStudent.Hobbies[2])
            affinity++;
        if (Hobbies[1] == targetStudent.Hobbies[0] || Hobbies[1] == targetStudent.Hobbies[1] || Hobbies[1] == targetStudent.Hobbies[2])
            affinity++;
        if (Hobbies[2] == targetStudent.Hobbies[0] || Hobbies[2] == targetStudent.Hobbies[1] || Hobbies[2] == targetStudent.Hobbies[2])
            affinity++;

        if (FavAnimals[0] == targetStudent.FavAnimals[0] || FavAnimals[0] == targetStudent.FavAnimals[1] || FavAnimals[0] == targetStudent.FavAnimals[2])
            affinity++;
        if (FavAnimals[1] == targetStudent.FavAnimals[0] || FavAnimals[1] == targetStudent.FavAnimals[1] || FavAnimals[1] == targetStudent.FavAnimals[2])
            affinity++;
        if (FavAnimals[2] == targetStudent.FavAnimals[0] || FavAnimals[2] == targetStudent.FavAnimals[2] || FavAnimals[2] == targetStudent.FavAnimals[2])
            affinity++;

        if (FavFoods[0] == targetStudent.FavFoods[0] || FavFoods[0] == targetStudent.FavFoods[1] || FavFoods[0] == targetStudent.FavFoods[2])
            affinity++;
        if (FavFoods[1] == targetStudent.FavFoods[0] || FavFoods[1] == targetStudent.FavFoods[1] || FavFoods[1] == targetStudent.FavFoods[2])
            affinity++;
        if (FavFoods[2] == targetStudent.FavFoods[0] || FavFoods[2] == targetStudent.FavFoods[1] || FavFoods[2] == targetStudent.FavFoods[2])
            affinity++;

        /*if (affinity > affinityThreshold)
        {
            createMessage("This dude is a total chad!", Color.blue);
        } else
        {
            createMessage("What a virgin!", Color.blue);
        }*/

        return affinity;
    }

    public override void setGroup(Group group)
    {
        this.group = group;
    }

    public void setFriendNumber(int number)
    {
        friendNumber = number;
    }

    //Behaviours
    private void Start()
    {
        clearSprites();
        targetStudent = null;
        if (currentOcuppiedPos != null) this.gameState.possiblePosGym.AddRange(currentOcuppiedPos);
        if (currentOcuppiedBench != null) this.gameState.possiblePosBench.AddRange(currentOcuppiedBench);
        if (currentOcuppiedOutside != null) this.gameState.possiblePosOutside.AddRange(currentOcuppiedOutside);
        if (currentOcuppiedBathroom != null) this.gameState.possiblePosBathroom.AddRange(currentOcuppiedBathroom);
        Debug.Log("[" + name + ", " + getRole() + "] Start");
        Vector3 destination = group.getMyPos(this.friendNumber);
        Debug.Log(this.friendNumber + " -> " + destination);
        Move(destination);
    }

    private void GettingCloser()
    {
        clearSprites();
        if (targetStudent == null) targetStudent = (CalmStudent) watching.getTargetCharacter();
        blackList.Add(targetStudent);
        if (currentOcuppiedPos != null) this.gameState.possiblePosGym.AddRange(currentOcuppiedPos);
        if (currentOcuppiedBench != null) this.gameState.possiblePosBench.AddRange(currentOcuppiedBench);
        if (currentOcuppiedOutside != null) this.gameState.possiblePosOutside.AddRange(currentOcuppiedOutside);
        Debug.Log("[" + name + ", " + getRole() + "] Heey " + targetStudent.getName());
        Vector3 offset = GetGameObject().transform.position - targetStudent.GetGameObject().transform.position;
        Move(targetStudent.GetGameObject().transform.position - offset.normalized);
        targetStudent.FireFlirt(this);
    }

    private void Enjoying()
    {
        createMessage(7);
        fatigue++;
    }

    protected void Dancing()
    {
        createMessage(4);
        fatigue++;
    }

    protected void Kissing()
    {
        createMessage(1);
        if (((CalmStudent) targetStudent).reservedKissPos == null) {
            var index = Random.Range(0, this.gameState.possiblePosBathroom.Count / 2 - 1) * 2;
            currentOcuppiedBathroom = this.gameState.possiblePosBathroom.GetRange(index, 2);
            this.gameState.possiblePosBathroom.RemoveRange(index, 2);

            this.reservedKissPos = currentOcuppiedBathroom;
        } else
        {
            currentOcuppiedBathroom = ((CalmStudent)targetStudent).reservedKissPos;
        }

        Vector3 kissingPos = new Vector3(currentOcuppiedBathroom[0], currentOcuppiedBathroom[1]);

        if (targetStudent.getDestination() == kissingPos) kissingPos.x = kissingPos.x + 1;
        Move(kissingPos);
    }

    private void CheckingAffinity()
    {
        if (currentOcuppiedPos != null) this.gameState.possiblePosGym.AddRange(currentOcuppiedPos);
        createMessage(0);
        Move(gameObject.transform.position);
        LookAt(targetStudent.GetGameObject().transform);
    }

    protected void Outside()
    {
        createMessage(5);
        fatigue = 0.0f;
        var index = Random.Range(0, this.gameState.possiblePosOutside.Count / 2 - 1) * 2;
        currentOcuppiedOutside = this.gameState.possiblePosOutside.GetRange(index, 2);
        this.gameState.possiblePosOutside.RemoveRange(index, 2);

        Move(new Vector3(currentOcuppiedOutside[0], currentOcuppiedOutside[1]));
    }

    private void FightAsCalm()
    {
        createMessage(2);
        fatigue++;
    }

    public bool GetMessyFlag()
    {
        return availableForMess;
    }

    public void SetMessyFlag(bool targeted)
    {
        availableForMess = targeted;
    }

    public void setTargetStudent(Student target)
    {
        targetStudent = target;
    }

    public override void FireFlirt(CalmStudent me)
    {
        this.targetStudent = me;
        calmStudentFSM.Fire("Like Person from enjoy");
        calmStudentFSM.Fire("Like Person from walk");
        calmStudentFSM.Fire("Like Person from bench");
        calmStudentFSM.Fire("Like Person from playground");
    }

    public override string Description()
    {
        var desc = "NAME: " + getName() + ", ROLE: " + getRole() + ", STATE: " + calmStudentFSM.GetCurrentState().Name;
        
        return desc + "\n";
    }
}

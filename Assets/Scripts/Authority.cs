using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Authority : Character
{
    //parameters
    protected StateMachineEngine doorSubFSM;
    private bool isNew = true;

    protected bool newSomeone = false;

    #region Stats
    protected float strictness;
    protected float thirst;
    #endregion

    //methods
    public Authority(string name, Genders gender, Transform obj) : base(name, gender, obj)
    {
        CreateDoorSubStateMachine();
    }

    private void CreateDoorSubStateMachine()
    {
        doorSubFSM = new StateMachineEngine(true);

        // Perceptions
        WatchingPerception seeCalmStudent = doorSubFSM.CreatePerception<WatchingPerception>(new WatchingPerception(this.gameObject, "CalmStudent", this.gameObject.GetComponent<PolygonCollider2D>()));
        Perception isNewStudent = doorSubFSM.CreatePerception<ValuePerception>(() => isNew);
        Perception seeNewStudent = doorSubFSM.CreateAndPerception<AndPerception>(seeCalmStudent, isNewStudent);
        Perception timer = doorSubFSM.CreatePerception<TimerPerception>(2);

        // States
        State waitingState = doorSubFSM.CreateEntryState("Waiting for someone", Waiting);
        State welcomeState = doorSubFSM.CreateState("Welcome", Welcome);

        // Transitions
        doorSubFSM.CreateTransition("Someone new arrives", waitingState, seeNewStudent, welcomeState);
        doorSubFSM.CreateTransition("Welcome finished", welcomeState, timer, waitingState);
    }

    //Common behaviours: Organizer Students and Teachers
    protected void Waiting()
    {
        Debug.Log("[" + name + ", " + getRole() + "] Waiting for someone to come...");
        this.Move(new Vector3(-7, -1));

        Text[] texts = GameObject.FindObjectsOfType<Text>();
        foreach (Text txt in texts)
        {
            if(txt.text == "Welcome to the party!")
            {
                GameObject.Destroy(txt);
            }
        }
    }

    protected void Welcome()
    {
        Debug.Log("[" + name + ", " + getRole() + "] Welcome to the party!");
        createMessage("Welcome to the party!", Color.blue);
        isNew = false;
    }

    protected void Patrol()
    {
        Debug.Log("[" + name + ", " + getRole() + "] I'm watching you!");
    }

    protected void ChaseStudent()
    {
        Debug.Log("[" + name + ", " + getRole() + "] Come back here, you little...");
    }

    public void createMessage(string text, Color color)
    {
        if (color == null)
        {
            color = Color.green;
        }
        if (text == null)
        {
            text = "";
        }

        GameObject newText = new GameObject(text.Replace(" ", "-"), typeof(RectTransform));
        var newTextComp = newText.AddComponent<Text>();
        //newText.AddComponent<CanvasRenderer>();

        //Text newText = transform.gameObject.AddComponent<Text>();
        newTextComp.text = text;
        newTextComp.color = color;
        newTextComp.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        newTextComp.alignment = TextAnchor.MiddleCenter;
        newTextComp.fontSize = 10;

        newText.transform.SetParent(GameObject.FindObjectOfType<Canvas>().transform);
        newText.transform.localPosition = new Vector3(-255, -18);
    }
}

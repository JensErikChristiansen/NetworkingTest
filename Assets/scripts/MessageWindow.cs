using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// messagewindow is a class that handles all the chat
public class MessageWindow : ScriptableObject
{

    List<MessageItem> messages = new List<MessageItem>();
    Vector2 size;
    float scrollBarWidth;
    float messageWidth;
    float lastTotalMessageHeight;
    Vector2 scrollPosition = new Vector2(0, 0);
    public bool pauseAutoScroll = false;
    public GUISkin skin;

    bool useCustomStyle = true;


    // message item class is one message for the message wiindow
    public abstract class MessageItem
    {
        public GUIStyle style;
        public float timeRemaining;
        public delegate void OnClicked(MessageItem item);
        public event OnClicked OnClickedAction;

        public MessageItem(float timeRemaining)
        {
            this.timeRemaining = timeRemaining;
        }
        public abstract void Draw(float yPosition, float width, ref float height);
        //Required so that classes that derive this class have access to the event OnClickedAction.
        //Events can only be called from the class they're defined in.
        protected virtual void RaiseOnClickedEvent(MessageItem item)
        {
            OnClickedAction(item);
        }
    }

    public class StringMessageItem : MessageItem
    {
        string text;

        public StringMessageItem(float timeToDisplay, string text) : base(timeToDisplay)
        {
            this.text = text;
        }

        public override void Draw(float yPosition, float width, ref float height)
        {
            GUIContent textContent = new GUIContent(text);
            height = style.CalcHeight(textContent, width);
            GUI.Label(new Rect(0, yPosition - height, width, height), textContent, style);
        }
    }

    public class LocationMessageItem : MessageItem
    {

        string text;
        Vector3 location;

        public LocationMessageItem(float timeToDisplay, string text, Vector3 location,
                                   OnClicked callbackOnClicked) : base(timeToDisplay)
        {
            this.text = text;
            this.location = location;
            this.OnClickedAction += callbackOnClicked;
        }

        public Vector3 GetLocation()
        {
            return location;
        }

        public override void Draw(float yPosition, float width, ref float height)
        {
            GUIContent textContent = new GUIContent(text);
            height = style.CalcHeight(textContent, width);
            if (GUI.Button(new Rect(0, yPosition - height, width, height), textContent))
            {
                base.RaiseOnClickedEvent(this);
                //this.timeRemaining = 0; //If we want to remove a message when it's clicked, set its time to 0
            }

        }
    }

    public MessageWindow()
    {
        
    }

    public void createParameters(Vector2 size, GUISkin skin)
    {
        this.size = size;
        this.skin = skin;
        if (skin.FindStyle("messageitem") == null)
        {
            Debug.Log("Skin does not contain 'messageitem' style, reverting to default.");
            useCustomStyle = false;
        }
        scrollBarWidth = skin.verticalScrollbar.fixedWidth;
        messageWidth = size.x - scrollBarWidth - 1;
        lastTotalMessageHeight = size.y;

    }

    public MessageWindow(Vector2 size, GUISkin skin)
    {
        this.size = size;
        this.skin = skin;
        if (skin.FindStyle("messageitem") == null)
        {
            Debug.Log("Skin does not contain 'messageitem' style, reverting to default.");
            useCustomStyle = false;
        }
        scrollBarWidth = skin.verticalScrollbar.fixedWidth;
        messageWidth = size.x - scrollBarWidth - 1;
        lastTotalMessageHeight = size.y;

    }

    public MessageItem AddMessage(MessageItem item)
    {
        this.messages.Add(item);
        if (!pauseAutoScroll)
        {
            scrollPosition.y = lastTotalMessageHeight;
        }

        if (useCustomStyle)
            item.style = skin.GetStyle("messageitem");
        else
            item.style = skin.label;

        return item;
    }

    public void AddMessage(float secondsToDisplay, string text)
    {
        AddMessage(new StringMessageItem(secondsToDisplay, text));
    }

    public void CountDownTimers()
    {
        List<MessageItem> deadMessages = new List<MessageItem>();
        foreach (MessageItem messageItem in messages)
        {
            messageItem.timeRemaining -= Time.deltaTime;
            if (messageItem.timeRemaining < 0)
            {
                deadMessages.Add(messageItem);
            }
        }
        foreach (MessageItem deadMessage in deadMessages)
        {
            messages.Remove(deadMessage);
        }
    }

    public void Draw(Vector2 position)
    {
        //Group our element to itself
        Rect windowRect = new Rect(position.x, position.y, size.x, size.y);
        GUI.BeginGroup(windowRect);
        GUI.Box(new Rect(0, 0, size.x, size.y), ""); //Draw a background box

        //Scroll view to "buffer" messages for later review
        scrollPosition = GUI.BeginScrollView(new Rect(0, 0, size.x, size.y),
                                         scrollPosition, new Rect(0, 0, messageWidth,
                                          lastTotalMessageHeight));

        //Keep track of height for future rendering
        float yPos = lastTotalMessageHeight;
        float totalMessageHeight = 0;
        float lastHeight = 0;

        //Each message draws itself, and sets a height variable so we 
        // can keep track of where we're at on screen.
        foreach (MessageItem messageItem in Enumerable.Reverse(messages))
        {
            messageItem.Draw(yPos, messageWidth, ref lastHeight);
            yPos -= lastHeight;
            totalMessageHeight += lastHeight;
        }
        //The last total message height is used to render the scroll view, 
        // and gives us a starting point for drawing next time we render each item
        //Use the size of the window or the size of the messages, which ever is greater
        lastTotalMessageHeight = Mathf.Max(totalMessageHeight, size.y);

        GUI.EndScrollView();
        GUI.EndGroup();
    }



    //--------------------------------------------------------

}

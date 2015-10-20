using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// MessageWindow is a class that handles all the chat between the clients.
/// MessageWindow GUI is a prototype for the real project, to show if
/// the networking component and system that we plan to use is
/// working properly.
/// </summary>
public class MessageWindow : ScriptableObject
{
    /// <summary>
    /// An array list of messages.
    /// </summary>
    List<MessageItem> messages = new List<MessageItem>();
    /// <summary>
    /// The size of the MessageWindow.
    /// </summary>
    Vector2 size;
    /// <summary>
    /// The width of the scrollbar for the MessageWindow.
    /// </summary>
    float scrollBarWidth;
    /// <summary>
    /// The width of the message.
    /// </summary>
    float messageWidth;
    /// <summary>
    /// The total message height for the last message.
    /// </summary>
    float lastTotalMessageHeight;
    /// <summary>
    /// The position for the scroll for the MessageWindow.
    /// </summary>
    Vector2 scrollPosition = new Vector2(0, 0);
    /// <summary>
    /// Whether it should auto scroll or not.
    /// </summary>
    public bool pauseAutoScroll = false;
    /// <summary>
    /// The GUISkin for the MessageWindow.
    /// </summary>
    public GUISkin skin;

    /// <summary>
    /// Wether or not it will use a custom style.
    /// </summary>
    bool useCustomStyle = true;

    /// <summary>
    /// Message item class is one message for the message window.
    /// </summary>
    public abstract class MessageItem
    {
        /// <summary>
        /// The style of the GUI for a MessageItem.
        /// </summary>
        public GUIStyle style;
        /// <summary>
        /// The remaining time for a MessageItem.
        /// </summary>
        public float timeRemaining;
        /// <summary>
        /// A pointer to the ClickMethod for the MessageItem.
        /// </summary>
        /// <param name="item">A MessageItem.</param>
        public delegate void OnClicked(MessageItem item);
        /// <summary>
        /// A click event for the message item.
        /// </summary>
        public event OnClicked OnClickedAction;

        /// <summary>
        /// The basic constructor of a MessageItem.
        /// </summary>
        /// <param name="timeRemaining">The time remaining for a MessageItem.</param>
        public MessageItem(float timeRemaining)
        {
            this.timeRemaining = timeRemaining;
        }

        /// <summary>
        /// The method that draws the MessageWindow on the screen.
        /// </summary>
        /// <param name="yPosition">The y position to draw at.</param>
        /// <param name="width">The width to draw at.</param>
        /// <param name="height">The height to draw at.</param>
        public abstract void Draw(float yPosition, float width, ref float height);

        /// <summary>
        /// Required so that classes that derive this class have access to the event OnClickedAction.
        /// Events can only be called from the class they're defined in.
        /// </summary>
        /// <param name="item">A MessageItem.</param>
        protected virtual void RaiseOnClickedEvent(MessageItem item)
        {
            OnClickedAction(item);
        }
    }

    /// <summary>
    /// A message string item which is one of the types of messages you can send with message window.
    /// </summary>
    public class StringMessageItem : MessageItem
    {
        /// <summary>
        /// The text that is sent between clients.
        /// </summary>
        string text;

        /// <summary>
        /// The basic constructor for a StringMessageItem.
        /// </summary>
        /// <param name="timeToDisplay">The time to display the message.</param>
        /// <param name="text">The text that the client sent.</param>
        public StringMessageItem(float timeToDisplay, string text) : base(timeToDisplay)
        {
            this.text = text;
        }

        /// <summary>
        /// The method that draws the MessageWindow on the screen.
        /// </summary>
        /// <param name="yPosition">The y position to draw at.</param>
        /// <param name="width">The width to draw at.</param>
        /// <param name="height">The height to draw at.</param>
        public override void Draw(float yPosition, float width, ref float height)
        {
            GUIContent textContent = new GUIContent(text);
            height = style.CalcHeight(textContent, width);
            GUI.Label(new Rect(0, yPosition - height, width, height), textContent, style);
        }
    }

    /// <summary>
    /// Location on the message item.
    /// </summary>
    public class LocationMessageItem : MessageItem
    {
        /// <summary>
        /// The text the client sent.
        /// </summary>
        string text;
        /// <summary>
        /// The location of the text sent.
        /// </summary>
        Vector3 location;

        /// <summary>
        /// A basic constructor for a message item.
        /// </summary>
        /// <param name="timeToDisplay">The time to display the message.</param>
        /// <param name="text">The text that the client sent.</param>
        /// <param name="location">The location of the message item.</param>
        /// <param name="callbackOnClicked">A pointer to the OnClicked method.</param>
        public LocationMessageItem(float timeToDisplay, string text, Vector3 location,
                                   OnClicked callbackOnClicked) : base(timeToDisplay)
        {
            this.text = text;
            this.location = location;
            this.OnClickedAction += callbackOnClicked;
        }

        /// <summary>
        /// Gets the location of the message.
        /// </summary>
        /// <returns>Returns the location of the message.</returns>
        public Vector3 GetLocation()
        {
            return location;
        }

        /// <summary>
        /// Draws the message on the message window
        /// </summary>
        /// <param name="yPosition">The y position to draw at.</param>
        /// <param name="width">The width to draw at.</param>
        /// <param name="height">The height to draw at.</param>
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

    /// <summary>
    /// Message window set up helper. Need this in order to initialize the MessageWindow.
    /// </summary>
    /// <param name="size">Takes in the size for the MessageWindow.</param>
    /// <param name="skin">Takes in the GUI for MessageWindow.</param>
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

    /// <summary>
    /// Basic constructor for creating a new MessageWindow.
    /// </summary>
    /// <param name="size">Takes in the size for the MessageWindow.</param>
    /// <param name="skin">Takes in the GUI for MessageWindow.</param>
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

    /// <summary>
    /// Adds a message to the MessageWindow.
    /// </summary>
    /// <param name="item">A MessageItem.</param>
    /// <returns>Returns the MessageItem.</returns>
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

    /// <summary>
    /// Adds a new string message to the MessageWindow.
    /// </summary>
    /// <param name="secondsToDisplay">The seconds to display.</param>
    /// <param name="text">The text sent.</param>
    public void AddMessage(float secondsToDisplay, string text)
    {
        AddMessage(new StringMessageItem(secondsToDisplay, text));
    }

    /// <summary>
    /// Begins count down timers for dead messages.
    /// </summary>
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

    /// <summary>
    /// Draws the MessageWindow according to the specs.
    /// </summary>
    /// <param name="position">The position to draw at.</param>
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

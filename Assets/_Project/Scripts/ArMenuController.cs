using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Understanding Menu Transitions:
   //- When the toggle button is pressed (`OnTogglePress`), it triggers the `toggleMenu` method.
   //- If the menu is closed, `openMenu` is called. It initializes a transition (`StateType.OpenTransition`) where the menu buttons are tweened to their target positions.
   //- If the menu is open, `closeMenu` is called. It initializes a transition (`StateType.CloseTransition`) where the menu buttons are tweened back to the toggle button's position.
   //- During transitions, the `Update` method is called to update the UI elements' positions until the transition is complete.
//Button Labels:
   //- The code also toggles the visibility of button labels (`showButtonLabels` and `hideButtonLabels`) based on the menu state.

//Path Rendering:
   //- The `OnLineRenderPress` method toggles between rendering a route path or a line to the target based on the `MenuController` settings.

//Overall, this script provides a flexible and animated menu system for controlling various actions within a Unity application.
   




namespace ARLocation.MapboxRoutes.SampleProject
{
    public class Tween
    {
        Vector3 start;
        Vector3 end;
        Vector3 current;
        float speed;
        float t;

        public Vector3 Position => current;

        public Tween(Vector3 startPos, Vector3 endPos, float tweenSpeed = 1)
        {
            start = startPos;
            end = endPos;
            speed = tweenSpeed;
            //`Tween` class represents a simple linear interpolation between two Vector3 positions.
        }

        public bool Update()
        {
            current = start * (1 - t) + end * t;

            t += Time.deltaTime * speed;

            if (t > 1)
            {
                return true;
            }

            return false;
        }
    }

    public class TweenRectTransform
    {
        public RectTransform Rt;
        public Vector3 PositionStart;
        public Vector3 PositionEnd;
        public Quaternion RotationStart;
        public Quaternion RotationEnd;
        //`TweenRectTransform` class extends `Tween` and is specifically designed to tween between two `RectTransform` positions and rotations.

        public TweenRectTransform(RectTransform rectTransform, Vector3 targetPosition, Quaternion targetRotation)
        {
            Rt = rectTransform;
            PositionStart = rectTransform.position;
            RotationStart = rectTransform.rotation;
            PositionEnd = targetPosition;
            RotationEnd = targetRotation;
        }
    }

    public class TweenRectTransformGroup
    { //`TweenRectTransformGroup` manages a group of `TweenRectTransform` instances and provides easing functions for smoother transitions.
        public enum EaseFunc
        {
            Linear,
            EaseOutBack,
            EaseInCubic,
        }

        public List<TweenRectTransform> Elements = new List<TweenRectTransform>();
        float speed;
        float t;
        Func<float, float, float, float> easeFunc;

        public TweenRectTransformGroup(float speed, EaseFunc easeFuncType)
        {
            this.speed = speed;

            switch (easeFuncType)
            {
                case EaseFunc.EaseOutBack:
                    this.easeFunc = EaseOutBack;
                    break;

                case EaseFunc.EaseInCubic:
                    this.easeFunc = EaseInCubic;
                    break;

                case EaseFunc.Linear:
                    this.easeFunc = EaseLinear;
                    break;
            }
        }

        public Vector3 ease(Vector3 start, Vector3 end, float t)
        {
            var x = easeFunc(start.x, end.x, t);
            var y = easeFunc(start.y, end.y, t);
            var z = easeFunc(start.z, end.z, t);

            return new Vector3(x, y, z);
        }

        public bool Update()
        {
            foreach (var e in Elements)
            {
                e.Rt.position = ease(e.PositionStart, e.PositionEnd, t); //e.PositionStart * (1 - t) + e.PositionEnd * t;
                e.Rt.rotation = Quaternion.Lerp(e.RotationStart, e.RotationEnd, t);
            }

            t += speed * Time.deltaTime;

            if (t > 1)
            {
                foreach (var e in Elements)
                {
                    e.Rt.position = e.PositionEnd;
                    e.Rt.rotation = e.RotationEnd;
                }

                return true;
            }

            return false;
        }

        public static float EaseOutBack(float start, float end, float value)
        {
            float s = 1.70158f;
            end -= start;
            value = (value) - 1;
            return end * ((value) * value * ((s + 1) * value + s) + 1) + start;
        }

        public static Vector3 EaseOutBack(Vector3 start, Vector3 end, float t)
        {
            float x = EaseOutBack(start.x, end.x, t);
            float y = EaseOutBack(start.y, end.y, t);
            float z = EaseOutBack(start.z, end.z, t);

            return new Vector3(x, y, z);
        }

        public static float EaseInCubic(float start, float end, float value)
        {
            end -= start;
            return end * value * value * value + start;
        }

        public static float EaseLinear(float start, float end, float value)
        {
            return start * (1 - value) * end * value;
        }

    }

    public class ArMenuController : MonoBehaviour
    { //This class manages the menu system and its interactions.
        public enum StateType
        { //- It defines an enum `StateType` representing the different states of the menu (Closed, Open, OpenTransition, CloseTransition).
            Closed,
            Open,
            OpenTransition,
            CloseTransition
        }

        [System.Serializable]
        public class StateData
        { //It contains nested classes for `StateData`, `ElementsData`, and `SettingsData` which hold references to UI elements and settings relevant to the menu.
            public StateType CurrentState = StateType.Closed;
            public TweenRectTransformGroup tweenGroup;
        }

        [System.Serializable]
        public class ElementsData
        {
            public Button BtnToggle;
            public Button BtnNext;
            public Button BtnPrev;
            public Button BtnRestart;
            public Button BtnExit;
            public Button BtnLineRender;

            public Text LabelNext;
            public Text LabelPrev;
            public Text LabelRestart;
            public Text LabelSearch;
            public Text LabelTargetRender;

            public RectTransform TargetNext;
            public RectTransform TargetPrev;
            public RectTransform TargetRestart;
            // public RectTransform TargetExit;
            // public RectTransform TargetLineRender;
        }

        [System.Serializable]
        public class SettingsData
        {
            public MapboxRoute MapboxRoute;
            public MenuController MenuController;
            public float TransitionSpeed = 2.0f;
        }

        public SettingsData Settings;
        public ElementsData Elements;
        private StateData s = new StateData();

        public void Awake()
        { //The `Awake` method initializes the menu state and shows only the toggle button.
            s = new StateData();

            showOnlyToggleButton();
        }

        void showOnlyToggleButton()
        {
            // Elements.BtnExit.gameObject.SetActive(false);
            // Elements.BtnLineRender.gameObject.SetActive(false);
            Elements.BtnNext.gameObject.SetActive(false);
            Elements.BtnPrev.gameObject.SetActive(false);
            Elements.BtnRestart.gameObject.SetActive(false);
            Elements.BtnToggle.gameObject.SetActive(true);
        }

        void showAllButtons()
        {
            // Elements.BtnExit.gameObject.SetActive(true);
            // Elements.BtnLineRender.gameObject.SetActive(true);
            Elements.BtnNext.gameObject.SetActive(true);
            Elements.BtnPrev.gameObject.SetActive(true);
            Elements.BtnRestart.gameObject.SetActive(true);
            Elements.BtnToggle.gameObject.SetActive(true);
        }

        public void OnEnable()
        { //The `OnEnable` and `OnDisable` methods handle adding and removing event listeners for button clicks.
            Elements.BtnToggle.onClick.AddListener(OnTogglePress);
            Elements.BtnNext.onClick.AddListener(OnNextPress);
            Elements.BtnPrev.onClick.AddListener(OnPrevPress);
            Elements.BtnRestart.onClick.AddListener(OnRestartPress);
            Elements.BtnExit.onClick.AddListener(OnSearchPress);
            Elements.BtnLineRender.onClick.AddListener(OnLineRenderPress);

            updateLineRenderButtonLabel();
        }

        public void OnDisable()
        { //The `OnEnable` and `OnDisable` methods handle adding and removing event listeners for button clicks.
            Elements.BtnToggle.onClick.RemoveListener(OnTogglePress);
            Elements.BtnNext.onClick.RemoveListener(OnNextPress);
            Elements.BtnPrev.onClick.RemoveListener(OnPrevPress);
            Elements.BtnRestart.onClick.RemoveListener(OnRestartPress);
            // Elements.BtnExit.onClick.RemoveListener(OnSearchPress);
            // Elements.BtnLineRender.onClick.RemoveListener(OnSearchPress);
        }

        private void updateLineRenderButtonLabel()
        { //There are methods for handling button clicks (`OnLineRenderPress`, `OnSearchPress`, `OnRestartPress`, `OnPrevPress`, `OnNextPress`, `OnTogglePress`).
            var mc = Settings.MenuController;

            if (mc.PathRendererType == MenuController.LineType.Route)
            {
                Elements.LabelTargetRender.text = "Route Path";
            }
            else
            {
                Elements.LabelTargetRender.text = "Line To Target";
            }

        }

        private void OnLineRenderPress()
        { //There are methods for handling button clicks (`OnLineRenderPress`, `OnSearchPress`, `OnRestartPress`, `OnPrevPress`, `OnNextPress`, `OnTogglePress`).
            var mc = Settings.MenuController;

            if (mc.PathRendererType == MenuController.LineType.Route)
            {
                mc.PathRendererType = MenuController.LineType.NextTarget;
                Elements.LabelTargetRender.text = "Line To Target";
            }
            else
            {
                mc.PathRendererType = MenuController.LineType.Route;
                Elements.LabelTargetRender.text = "Route Path";
            }
        }


        private void OnSearchPress()
        {//There are methods for handling button clicks (`OnLineRenderPress`, `OnSearchPress`, `OnRestartPress`, `OnPrevPress`, `OnNextPress`, `OnTogglePress`).
            Settings.MenuController.EndRoute();
        }

        private void OnRestartPress()
        { //There are methods for handling button clicks (`OnLineRenderPress`, `OnSearchPress`, `OnRestartPress`, `OnPrevPress`, `OnNextPress`, `OnTogglePress`).
            Settings.MapboxRoute.ClosestTarget();
        }

        private void OnPrevPress()
        { //There are methods for handling button clicks (`OnLineRenderPress`, `OnSearchPress`, `OnRestartPress`, `OnPrevPress`, `OnNextPress`, `OnTogglePress`).
            Settings.MapboxRoute.PrevTarget();
        }

        private void OnNextPress()
        { //There are methods for handling button clicks (`OnLineRenderPress`, `OnSearchPress`, `OnRestartPress`, `OnPrevPress`, `OnNextPress`, `OnTogglePress`).
            Settings.MapboxRoute.NextTarget();
        }

        private void OnTogglePress()
        { //There are methods for handling button clicks (`OnLineRenderPress`, `OnSearchPress`, `OnRestartPress`, `OnPrevPress`, `OnNextPress`, `OnTogglePress`).
            toggleMenu();
        }

        void toggleMenu()
        { //The `toggleMenu`, `openMenu`, and `closeMenu` methods control the opening and closing animations of the menu.
            if (s.CurrentState == StateType.Closed)
            {
                openMenu();
            }
            else if (s.CurrentState == StateType.Open)
            {
                closeMenu();
            }
        }

        void openMenu()
        {
            switch (s.CurrentState)
            {
                case StateType.Closed:

                    showAllButtons();

                    s.tweenGroup = new TweenRectTransformGroup(Settings.TransitionSpeed, TweenRectTransformGroup.EaseFunc.EaseInCubic);

                    s.tweenGroup.Elements.Add(new TweenRectTransform(Elements.BtnNext.GetComponent<RectTransform>(), Elements.TargetNext.position, Quaternion.identity));
                    s.tweenGroup.Elements.Add(new TweenRectTransform(Elements.BtnPrev.GetComponent<RectTransform>(), Elements.TargetPrev.position, Quaternion.identity));
                    s.tweenGroup.Elements.Add(new TweenRectTransform(Elements.BtnRestart.GetComponent<RectTransform>(), Elements.TargetRestart.position, Quaternion.identity));
                    // s.tweenGroup.Elements.Add(new TweenRectTransform(Elements.BtnExit.GetComponent<RectTransform>(), Elements.TargetExit.position, Quaternion.identity));
                    // s.tweenGroup.Elements.Add(new TweenRectTransform(Elements.BtnLineRender.GetComponent<RectTransform>(), Elements.TargetLineRender.position, Quaternion.identity));

                    s.tweenGroup.Elements.Add(new TweenRectTransform(Elements.BtnToggle.GetComponent<RectTransform>(), Elements.BtnToggle.GetComponent<RectTransform>().position, Quaternion.Euler(0, 0, 180)));

                    //s.BtnNextTween = new Tween(Elements.BtnNext.GetComponent<RectTransform>().position, Elements.TargetNext.position, Settings.TransitionSpeed);
                    //s.BtnPrevTween = new Tween(Elements.BtnPrev.GetComponent<RectTransform>().position, Elements.TargetPrev.position, Settings.TransitionSpeed);
                    //s.BtnRestartTween = new Tween(Elements.BtnRestart.GetComponent<RectTransform>().position, Elements.TargetRestart.position, Settings.TransitionSpeed);
                    //s.BtnExitTween = new Tween(Elements.BtnExit.GetComponent<RectTransform>().position, Elements.TargetExit.position, Settings.TransitionSpeed);

                    Elements.BtnToggle.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 180);

                    s.CurrentState = StateType.OpenTransition;

                    break;
            }
        }

        void closeMenu()
        {
            switch (s.CurrentState)
            {
                case StateType.Open:

                    s.tweenGroup = new TweenRectTransformGroup(Settings.TransitionSpeed, TweenRectTransformGroup.EaseFunc.EaseInCubic);

                    var togglerRt = Elements.BtnToggle.GetComponent<RectTransform>();

                    s.tweenGroup.Elements.Add(new TweenRectTransform(Elements.BtnNext.GetComponent<RectTransform>(), togglerRt.position, Quaternion.identity));
                    s.tweenGroup.Elements.Add(new TweenRectTransform(Elements.BtnPrev.GetComponent<RectTransform>(), togglerRt.position, Quaternion.identity));
                    s.tweenGroup.Elements.Add(new TweenRectTransform(Elements.BtnRestart.GetComponent<RectTransform>(), togglerRt.position, Quaternion.identity));
                    // s.tweenGroup.Elements.Add(new TweenRectTransform(Elements.BtnExit.GetComponent<RectTransform>(), togglerRt.position, Quaternion.identity));
                    // s.tweenGroup.Elements.Add(new TweenRectTransform(Elements.BtnLineRender.GetComponent<RectTransform>(), togglerRt.position, Quaternion.identity));

                    s.tweenGroup.Elements.Add(new TweenRectTransform(Elements.BtnToggle.GetComponent<RectTransform>(), Elements.BtnToggle.GetComponent<RectTransform>().position, Quaternion.Euler(0, 0, 0)));

                    //Elements.BtnToggle.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 0);

                    s.CurrentState = StateType.CloseTransition;

                    //showOnlyToggleButton();
                    //var pos = Elements.BtnToggle.GetComponent<RectTransform>().position;
                    //Elements.BtnNext.GetComponent<RectTransform>().position = pos;
                    //Elements.BtnPrev.GetComponent<RectTransform>().position = pos;
                    //Elements.BtnRestart.GetComponent<RectTransform>().position = pos;
                    //Elements.BtnExit.GetComponent<RectTransform>().position = pos;
                    //Elements.BtnToggle.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 0);
                    //s.CurrentState = StateType.Closed;
                    break;
            }
        }

        void Start()
        {

        }

        void showButtonLabels()
        {
            Elements.LabelNext.gameObject.SetActive(true);
            Elements.LabelPrev.gameObject.SetActive(true);
            Elements.LabelRestart.gameObject.SetActive(true);
            Elements.LabelSearch.gameObject.SetActive(true);
            Elements.LabelTargetRender.gameObject.SetActive(true);
        }

        void hideButtonLabels()
        {
            Elements.LabelNext.gameObject.SetActive(false);
            Elements.LabelPrev.gameObject.SetActive(false);
            Elements.LabelRestart.gameObject.SetActive(false);
            Elements.LabelTargetRender.gameObject.SetActive(false);

        }

        void Update()
        { //`Update` method updates the menu state based on the current transition state.
            switch (s.CurrentState)
            {
                case StateType.OpenTransition:
                    if (s.tweenGroup.Update())
                    {
                        showButtonLabels();
                        s.CurrentState = StateType.Open;
                    }

                    break;

                case StateType.CloseTransition:
                    if (s.tweenGroup.Update())
                    {
                        hideButtonLabels();
                        showOnlyToggleButton();
                        s.CurrentState = StateType.Closed;
                    }
                    break;
            }
        }
    }
}

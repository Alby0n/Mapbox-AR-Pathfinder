using UnityEngine;
using UnityEngine.UI;

namespace ARLocation.MapboxRoutes
{
    public class DefaultOnScreenTargetIndicator : AbstractOnScreenTargetIndicator
    {
        public enum TargetVisibilityState
        {
            None,
            Visible,
            OffUp,
            OffDown,
            OffLeft,
            OffRight
        }

        public enum ArrowDir
        {
            Left,
            Right
        }

        public Sprite ArrowSprite;
        public ArrowDir NeutralArrowDirection;
        public float Margin = 20;

        private RectTransform indicator;
        private Canvas canvas;
        private Camera cam;
        private Transform camTransform;
        private Renderer targetRenderer;
        private TargetVisibilityState targetVisibility;
        private bool initialized;

        public TargetVisibilityState TargetVisibility => targetVisibility;

        public override void Init(MapboxRoute route)
        {
            if (!initialized)
            {
                var canvasGo = new GameObject("[OnScreenTargetIndicatorCanvas]");
                var canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                var indicatorGo = new GameObject("[OnScreenTargetIndicatorImage]");
                indicatorGo.transform.parent = canvasGo.transform;
                var indicatorImage = indicatorGo.AddComponent<Image>();
                indicatorImage.sprite = ArrowSprite;
                indicator = indicatorGo.GetComponent<RectTransform>();

                cam = Camera.main;
                camTransform = cam.transform;

                initialized = true;
            }
        }

        bool isLeftOfCamera(Vector3 targetPos)
        {
            var camForward = camTransform.forward;
            var camPos = camTransform.position;
            return Vector3.Dot(Vector3.Cross(camForward, targetPos - camPos), (new Vector3(0, 1, 0))) < 0;
        }

        bool isBehindCamera(Vector3 targetPos)
        {
            var camForward = camTransform.forward;
            var relative = targetPos - camTransform.position;

            return Vector3.Dot(camForward, relative) < 0;
        }

        bool isVisible(Vector3 targetPos)
        {
            if (isBehindCamera(targetPos))
            {
                return false;
            }

            if (targetRenderer != null)
            {
                return GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(Camera.main), targetRenderer.bounds);
            }
            else
            {
                var p = cam.WorldToScreenPoint(targetPos);
                return (p.x >= 0 && p.x <= Screen.width) && (p.y >= 0 && p.y <= Screen.height);
            }
        }

        public override void OnRouteUpdate(SignPostEventArgs args)
        {
            bool isLeft = isLeftOfCamera(args.TargetPos);
            bool isBehind = isBehindCamera(args.TargetPos);
            bool isFront = !isBehind;
            bool isRight = !isLeft;

            if (isVisible(args.TargetPos))
            {
                this.indicator.gameObject.SetActive(false);
                targetVisibility = TargetVisibilityState.Visible;
                return;
            }
            else
            {
                this.indicator.gameObject.SetActive(true);
            }

            var p = cam.WorldToScreenPoint(args.TargetPos);

            if (p.x < 0)
            {
                targetVisibility = TargetVisibilityState.OffLeft;
            }
            else if (p.x >= Screen.width)
            {
                targetVisibility = TargetVisibilityState.OffRight;
            }
            else if (p.y < 0)
            {
                targetVisibility = isBehind ? TargetVisibilityState.OffUp : TargetVisibilityState.OffDown;
            }
            else
            {
                targetVisibility = isBehind ? TargetVisibilityState.OffDown : TargetVisibilityState.OffUp;
            }

            p.x = Mathf.Clamp(p.x, Margin, Screen.width - Margin);
            p.y = Mathf.Clamp(p.y, Margin, Screen.height - Margin);

            if (isBehind)
            {
                p.y = Screen.height - p.y;

                if (isLeft)
                {
                    p.x = Margin;
                    targetVisibility = TargetVisibilityState.OffLeft;
                }
                else
                {
                    p.x = Screen.width - Margin;
                    targetVisibility = TargetVisibilityState.OffRight;
                }
            }

            indicator.position = p;

            switch (targetVisibility)
            {
                case TargetVisibilityState.OffLeft:
                    if (NeutralArrowDirection == ArrowDir.Right)
                    {
                        indicator.rotation = Quaternion.AngleAxis(180, Vector3.forward);
                    }
                    else
                    {
                        indicator.rotation = Quaternion.identity;
                    }
                    break;

                case TargetVisibilityState.OffRight:
                    if (NeutralArrowDirection == ArrowDir.Right)
                    {
                        indicator.rotation = Quaternion.identity;
                    }
                    else
                    {
                        indicator.rotation = Quaternion.AngleAxis(180, Vector3.forward);
                    }
                    break;

                case TargetVisibilityState.OffUp:
                    if (NeutralArrowDirection == ArrowDir.Right)
                    {
                        indicator.rotation = Quaternion.AngleAxis(90, Vector3.forward);
                    }
                    else
                    {
                        indicator.rotation = Quaternion.AngleAxis(-90, Vector3.forward);
                    }
                    break;

                case TargetVisibilityState.OffDown:
                    if (NeutralArrowDirection == ArrowDir.Right)
                    {
                        indicator.rotation = Quaternion.AngleAxis(-90, Vector3.forward);
                    }
                    else
                    {
                        indicator.rotation = Quaternion.AngleAxis(90, Vector3.forward);
                    }
                    break;

                default:
                    indicator.rotation = Quaternion.identity;
                    break;
            }

        }
    }
}

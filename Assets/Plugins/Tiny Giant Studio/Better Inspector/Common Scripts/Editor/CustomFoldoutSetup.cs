using UnityEngine;
using UnityEngine.UIElements;

namespace TinyGiantStudio.BetterInspector
{
    public class CustomFoldoutSetup
    {
        public void SetupFoldout(GroupBox container, string toggleName = "FoldoutToggle")
        {
            Toggle toggle = container.Q<Toggle>(toggleName);
            GroupBox content = container.Q<GroupBox>("Content");

            SwitchContent(content, toggle.value, toggle, true);

            BindToggle(toggle, content);
        }

        private void BindToggle(Toggle toggle, GroupBox content)
        {
            toggle.RegisterValueChangedCallback(ev =>
            {
                SwitchContent(content, ev.newValue, toggle);
            });
        }

        /// <summary>
        /// Note to self. This is called twice at the start. Once false, then true
        /// </summary>
        /// <param name="content"></param>
        /// <param name="toggleStatus"></param>
        /// <param name="toggle"></param>
        public void SwitchContent(GroupBox content, bool toggleStatus, Toggle toggle = null, bool instant = false)
        {
            if (toggleStatus)
            {
                RevealContent(content);
            }
            else
            {
                if (!instant)
                    content.schedule.Execute(() => TurnOffContent(content, toggle)).ExecuteLater(50);
                else
                    TurnOffContent(content, toggle);

                content.style.opacity = 0;
                content.style.translate = new StyleTranslate(new Translate(0, -10));
                content.style.scale = new StyleScale(new Scale(new Vector3(1, 0, 1)));
            }
        }

        private void TurnOffContent(GroupBox content, Toggle toggle)
        {
            if (toggle != null)
                if (toggle.value)
                    return;
            content.style.display = DisplayStyle.None;
        }

        private void RevealContent(GroupBox content)
        {
            content.style.display = DisplayStyle.Flex;
            content.style.opacity = 1;
            content.style.translate = new StyleTranslate(new Translate(0, 0));
            content.style.scale = new StyleScale(new Scale(new Vector3(1, 1, 1)));
        }
    }
}
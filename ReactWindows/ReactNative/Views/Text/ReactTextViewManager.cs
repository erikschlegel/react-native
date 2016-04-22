using ReactNative.Reflection;
using ReactNative.UIManager;
using ReactNative.UIManager.Annotations;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;

namespace ReactNative.Views.Text
{
    /// <summary>
    /// The view manager for text views.
    /// </summary>
    public class ReactTextViewManager : BaseViewManager<TextBlock, ReactTextShadowNode>
    {
        private const string ReactClass = "RCTText";

        /// <summary>
        /// The name of the view manager.
        /// </summary>
        public override string Name
        {
            get
            {
                return ReactClass;
            }
        }

        /// <summary>
        /// Sets the text alignment.
        /// </summary>
        /// <param name="view">The view instance.</param>
        /// <param name="textAlign">The text alignment string.</param>
        [ReactProp(ViewProps.TextAlign)]
        public void SetTextAlign(TextBlock view, string textAlign)
        {
            if (textAlign != null)
            {
                if (textAlign.Equals("auto"))
                {
                    view.TextAlignment = TextAlignment.DetectFromContent;
                }
                else
                {
                    view.TextAlignment = EnumHelpers.Parse<TextAlignment>(textAlign);
                }
            }
        }

        /// <summary>
        /// Creates the shadow node instance.
        /// </summary>
        /// <returns>The shadow node instance.</returns>
        public override ReactTextShadowNode CreateShadowNodeInstance()
        {
            return new ReactTextShadowNode(false);
        }

        /// <summary>
        /// Updates the node with the changes to the inner virtual nodes.
        /// </summary>
        /// <param name="root">The view instance.</param>
        /// <param name="extraData">The aggregated virtual node changes.</param>
        public override void UpdateExtraData(TextBlock root, object extraData)
        {
            var inline = (Inline)extraData;
            root.Inlines.Clear();
            root.Inlines.Add(inline);
        }

        /// <summary>
        /// Creates the view instance.
        /// </summary>
        /// <param name="reactContext">The react context.</param>
        /// <returns>The view instance.</returns>
        protected override TextBlock CreateViewInstance(ThemedReactContext reactContext)
        {
            return new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.DetectFromContent,
            };
        }
    }
}

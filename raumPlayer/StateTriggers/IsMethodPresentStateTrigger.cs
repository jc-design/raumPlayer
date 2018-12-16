// Copyright (c) Morten Nielsen. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml;

namespace raumPlayer.StateTriggers
{
    /// <summary>
    /// Enables a state if a type is present on the device
    /// </summary>
    /// <remarks>
    /// <para>
    /// Example: Checking for hardware back button availability:
    /// <code lang="xaml"><triggers:IsMethodPresentStateTrigger TypeName="Windows.UI.Composition.Compositor" MethodName="CreateHostBackdropBrush" />
    /// </code>
    /// </para>
    /// </remarks>

    public class IsMethodPresentStateTrigger : StateTriggerBase, ITriggerValue
    {
        /// <summary>
        /// Gets or sets the name of the type.
        /// </summary>
        /// <remarks>
        /// Example: <c>Windows.Phone.UI.Input.HardwareButtons</c>
        /// </remarks>
        public string TypeName
        {
            get { return (string)GetValue(TypeNameProperty); }
            set { SetValue(TypeNameProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="TypeName"/> DependencyProperty
        /// </summary>
        public static readonly DependencyProperty TypeNameProperty = DependencyProperty.Register("TypeNameName", typeof(string), typeof(IsMethodPresentStateTrigger),new PropertyMetadata("", OnTypeNamePropertyChanged));

        private static void OnTypeNamePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = (IsMethodPresentStateTrigger)d;
            var typeName = (string)e.NewValue;
            var methodName = obj.MethodName;

            obj.IsActive = (!string.IsNullOrWhiteSpace(typeName) && !string.IsNullOrWhiteSpace(methodName) && ApiInformation.IsMethodPresent(typeName, methodName));
        }

        /// <summary>
        /// Gets or sets the name of the type.
        /// </summary>
        /// <remarks>
        /// Example: <c>Windows.Phone.UI.Input.HardwareButtons</c>
        /// </remarks>
        public string MethodName
        {
            get { return (string)GetValue(MethodNameProperty); }
            set { SetValue(MethodNameProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="MethodName"/> DependencyProperty
        /// </summary>
        public static readonly DependencyProperty MethodNameProperty = DependencyProperty.Register("MethodName", typeof(string), typeof(IsMethodPresentStateTrigger), new PropertyMetadata("", OnMethodNamePropertyChanged));

        private static void OnMethodNamePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = (IsMethodPresentStateTrigger)d;
            var methodName = (string)e.NewValue;
            var typeName = obj.TypeName;
            
            obj.IsActive = (!string.IsNullOrWhiteSpace(methodName) && !string.IsNullOrWhiteSpace(typeName) && ApiInformation.IsMethodPresent(typeName, methodName));
        }

        #region ITriggerValue

        private bool m_IsActive;

        /// <summary>
        /// Gets a value indicating whether this trigger is active.
        /// </summary>
        /// <value><c>true</c> if this trigger is active; otherwise, <c>false</c>.</value>
        public bool IsActive
        {
            get { return m_IsActive; }
            private set
            {
                if (m_IsActive != value)
                {
                    m_IsActive = value;
                    base.SetActive(value);
                    if (IsActiveChanged != null)
                        IsActiveChanged(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Occurs when the <see cref="IsActive" /> property has changed.
        /// </summary>

        public event EventHandler IsActiveChanged;
        #endregion ITriggerValue
    }
}

﻿#pragma checksum "..\..\..\CCSorterApp.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "613EF766636AAFF7868799D31B0D51D7C25D1219"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Sims_4_CC_Sorter;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace Sims_CC_Sorter {
    
    
    /// <summary>
    /// CCSorterApp
    /// </summary>
    public partial class CCSorterApp : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 17 "..\..\..\CCSorterApp.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox PickedLocation;
        
        #line default
        #line hidden
        
        
        #line 18 "..\..\..\CCSorterApp.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button BrowseForLocation;
        
        #line default
        #line hidden
        
        
        #line 28 "..\..\..\CCSorterApp.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.RadioButton radioButton_Sims2;
        
        #line default
        #line hidden
        
        
        #line 29 "..\..\..\CCSorterApp.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.RadioButton radioButton_Sims3;
        
        #line default
        #line hidden
        
        
        #line 30 "..\..\..\CCSorterApp.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.RadioButton radioButton_Sims4;
        
        #line default
        #line hidden
        
        
        #line 36 "..\..\..\CCSorterApp.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button findBrokenButton;
        
        #line default
        #line hidden
        
        
        #line 38 "..\..\..\CCSorterApp.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button renameSims2Packages;
        
        #line default
        #line hidden
        
        
        #line 41 "..\..\..\CCSorterApp.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock completionAlert;
        
        #line default
        #line hidden
        
        
        #line 45 "..\..\..\CCSorterApp.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button exitButton;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "6.0.5.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/Sims CC Sorter;component/ccsorterapp.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\CCSorterApp.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "6.0.5.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.PickedLocation = ((System.Windows.Controls.TextBox)(target));
            return;
            case 2:
            this.BrowseForLocation = ((System.Windows.Controls.Button)(target));
            
            #line 18 "..\..\..\CCSorterApp.xaml"
            this.BrowseForLocation.Click += new System.Windows.RoutedEventHandler(this.browseLocation_Click);
            
            #line default
            #line hidden
            return;
            case 3:
            this.radioButton_Sims2 = ((System.Windows.Controls.RadioButton)(target));
            return;
            case 4:
            this.radioButton_Sims3 = ((System.Windows.Controls.RadioButton)(target));
            return;
            case 5:
            this.radioButton_Sims4 = ((System.Windows.Controls.RadioButton)(target));
            return;
            case 6:
            this.findBrokenButton = ((System.Windows.Controls.Button)(target));
            
            #line 36 "..\..\..\CCSorterApp.xaml"
            this.findBrokenButton.Click += new System.Windows.RoutedEventHandler(this.findBroken_Click);
            
            #line default
            #line hidden
            return;
            case 7:
            this.renameSims2Packages = ((System.Windows.Controls.Button)(target));
            
            #line 38 "..\..\..\CCSorterApp.xaml"
            this.renameSims2Packages.Click += new System.Windows.RoutedEventHandler(this.renameSims2Packages_Click);
            
            #line default
            #line hidden
            return;
            case 8:
            this.completionAlert = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 9:
            this.exitButton = ((System.Windows.Controls.Button)(target));
            
            #line 45 "..\..\..\CCSorterApp.xaml"
            this.exitButton.Click += new System.Windows.RoutedEventHandler(this.exitButton_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}


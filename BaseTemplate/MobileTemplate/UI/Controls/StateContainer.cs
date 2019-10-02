//  Based on https://github.com/xDelivered-Patrick/Xamarin.Forms.Essentials/blob/master/Essentials/Controls/State/
//  Special thanks to Patrick McCurley from Binwell Ltd.

//  Distributed under Apache 2.0 Licence: http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MobileTemplate.UI.Controls {
	[ContentProperty("Content")] 
	public class StateCondition : View 
	{ 
		public object State { get; set; } 
		public View Content { get; set; } 
	}

	[ContentProperty("Conditions")] 
    public class StateContainer : ContentView { 
        public List<StateCondition> Conditions { get; } = new List<StateCondition>(); 
 
        public static readonly BindableProperty StateProperty = BindableProperty.Create(nameof(State), typeof(object), typeof(StateContainer), null, BindingMode.Default, null, StateChanged); 
 
		public object State 
		{ 
			get => GetValue(StateProperty);
			set => SetValue(StateProperty, value);
		}

        static async void StateChanged(BindableObject bindable, object oldValue, object newValue) 
        {
	        if (bindable is StateContainer parent) 
                await parent.ChooseStateProperty(newValue); 
        } 
 
        async Task ChooseStateProperty(object newValue) 
        { 
            if (Conditions.Count == 0) return; 
 
            try 
            {
                foreach (var stateCondition in Conditions.Where(stateCondition => stateCondition.State != null && stateCondition.State.ToString().Equals(newValue.ToString()))) { 
                    if (Content != null) 
                    { 
                        await Content.FadeTo(0, 100U); 
                        Content.IsVisible = false; 
                        await Task.Delay(30);
                    }
					
                    stateCondition.Content.Opacity = 0; 
                    Content = stateCondition.Content; 
                    Content.IsVisible = true; 
                    await Content.FadeTo(1); 
 
                    break; 
                } 
            } catch (Exception e) 
            { 
                Debug.WriteLine($"StateContainer ChooseStateProperty {newValue} error: {e}"); 
            } 
        } 
    }
}
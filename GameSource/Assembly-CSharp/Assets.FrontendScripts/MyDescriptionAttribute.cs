using System;

namespace Assets.FrontendScripts;

public class MyDescriptionAttribute : Attribute
{
	public virtual string Text { get; set; }
}

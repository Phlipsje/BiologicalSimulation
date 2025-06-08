using Implementations;

//Sets culture to US-English, specific language does not matter, but because we set this, using float.Parse and writing floats to file always use '.' as decimal point.
System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("en-US");
System.Threading.Thread.CurrentThread.CurrentCulture = ci;

using var program = new Main();
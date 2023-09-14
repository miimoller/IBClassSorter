using Microsoft.AspNetCore.Components.WebView.Maui;
using IBClassSorter.Data;
using IBClassSorter.Pages;

namespace IBClassSorter;

public static class MauiProgram
{


	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
            .ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();
		#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
#endif
		
		builder.Services.AddTransient<Pages.ClassPreferences>();
		builder.Services.AddTransient<FinalDisplay>();
		builder.Services.AddTransient<InputClasses>();
		builder.Services.AddTransient<InputStudents>();
		builder.Services.AddTransient<InputTeachers>();
		builder.Services.AddTransient<Instructions>();
		builder.Services.AddTransient<UpdateClassPreferences>();
		

		return builder.Build();
	}
}

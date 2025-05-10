using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Xamarin.Essentials;
using TestAppB.Services;
using TestAppB.Models;

namespace TestAppB.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class AchievementsPage : ContentPage
	{
		public AchievementsPage ()
		{
			InitializeComponent ();

            var list = AchievementService.AllAchievements;

            foreach (var ach in list)
            {
                ach.IsUnlocked = Preferences.Get($"Ach_{ach.Id}", false);
            }

            achievementsView.ItemsSource = list;
        }
	}
}
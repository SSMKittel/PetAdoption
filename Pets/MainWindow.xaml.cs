﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Timers;

namespace Pets
{
    public partial class MainWindow : Window
    {
        private readonly IPets allPets;
        private readonly PetService petService;

        private static readonly string TYPE_AWL_CAT = new LocalizedString("awl_type_Cat").ToString();
        private static readonly string TYPE_RSPCA_CAT = new LocalizedString("rspca_type_2").ToString();
        private static readonly string TYPE_RSPCA_KITTEN = new LocalizedString("rspca_type_15").ToString();
        private static readonly string TYPE_AWL_RABBIT = new LocalizedString("awl_type_Rabbit").ToString();
        private static readonly string TYPE_RSPCA_RABBIT = new LocalizedString("rspca_type_86").ToString();
        private static readonly string TYPE_AWL_DOG = new LocalizedString("awl_type_Dog").ToString();
        private static readonly string TYPE_RSPCA_DOG = new LocalizedString("rspca_type_3").ToString();
        private static readonly string TYPE_RSPCA_PUPPY = new LocalizedString("rspca_type_16").ToString();

        private const double INITIAL_DURATION = 1000 * 1 * 60;
        private const double AUTO_UPDATE_DURATION = 1000 * 1 * 60 * 60;

        private readonly Timer updateTimer;

        public MainWindow()
        {
            allPets = new PersistentPets();
            petService = new PetService(allPets);
            InitializeComponent();
            listView.ItemsSource = getPetList();

            updateTimer = new Timer(INITIAL_DURATION);
            updateTimer.Elapsed += fireTimer;
            updateTimer.AutoReset = false;
            updateTimer.Start();
        }

        private void fireTimer(object sender, ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(() => ButtonClick(sender, default(RoutedEventArgs)));
        }

        private async void ButtonClick(object sender, RoutedEventArgs e)
        {
            updateTimer.Stop();
            updateTimer.Interval = AUTO_UPDATE_DURATION;

            button.IsEnabled = false;
            await petService.Update();
            this.Dispatcher.Invoke(() => { if (lastUpdated != null) lastUpdated.Content = DateTime.Now; });
            this.Dispatcher.Invoke(updateListView);
            this.Dispatcher.Invoke(() => button.IsEnabled = true);
            updateTimer.Start();
        }

        private void updateListView()
        {
            listView.ItemsSource = getPetList();
            listView.InvalidateVisual();
            if(lastUpdated != null) lastUpdated.InvalidateVisual();
        }

        private IEnumerable<Pet> getPetList()
        {
            IEnumerable<Pet> pets = allPets.GetAll();
            if (shouldRemove(typeCat))
            {
                pets = pets.Where(notCat);
            }
            if (shouldRemove(typeRabbit))
            {
                pets = pets.Where(notRabbit);
            }
            if (shouldRemove(typeDog))
            {
                pets = pets.Where(notDog);
            }
            if (shouldRemove(typeOther))
            {
                pets = pets.Where(notOther);
            }
            return pets.OrderByDescending(x => x.LastChanged).ThenBy(x => x.Name).ThenBy(x => x.Id);
        }

        private static bool isCat(Pet pet)
        {
            return pet.Type == TYPE_AWL_CAT || pet.Type == TYPE_RSPCA_CAT || pet.Type == TYPE_RSPCA_KITTEN;
        }

        private static bool notCat(Pet pet)
        {
            return !isCat(pet);
        }

        private static bool isDog(Pet pet)
        {
            return pet.Type == TYPE_AWL_DOG || pet.Type == TYPE_RSPCA_DOG || pet.Type == TYPE_RSPCA_PUPPY;
        }

        private static bool notDog(Pet pet)
        {
            return !isDog(pet);
        }

        private static bool isRabbit(Pet pet)
        {
            return pet.Type == TYPE_AWL_RABBIT || pet.Type == TYPE_RSPCA_RABBIT;
        }

        private static bool notRabbit(Pet pet)
        {
            return !isRabbit(pet);
        }

        private static bool isOther(Pet pet)
        {
            return notCat(pet) && notDog(pet) && notRabbit(pet);
        }

        private static bool notOther(Pet pet)
        {
            return !isOther(pet);
        }

        private static bool shouldRemove(CheckBox cb)
        {
            if(cb == null) return true;
            bool? val = cb.IsChecked;
            if (!val.HasValue) return true;
            return !val.Value;
        }

        private void typeChecked(object sender, RoutedEventArgs e)
        {
            updateListView();
        }
    }

}

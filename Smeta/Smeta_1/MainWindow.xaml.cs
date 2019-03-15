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
using MahApps.Metro.Controls;
using Ninject;
using System.Data.Entity;
using Microsoft.Win32;
using System.Xml.Linq;
using Smeta_DB;

namespace Smeta_1
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : MetroWindow
	{

		//string sLogin;
		//string sPassword;
		public static string sRole; // Роль пользователя, вошедшего в систему
		public static int idSotrudn; // ID пользователя, вошедшего в систему

		SmetaEntities1 context = new SmetaEntities1();

		public MainWindow()
		{
			InitializeComponent();
		}

		public event EventHandler ClickAdmin;
		public event EventHandler ClickUser;
		private void btnLogin_Click(object sender, RoutedEventArgs e)
		{
			//using (SmetaEntities Исполнитель = new SmetaEntities())
			//{
			//	string login = textBoxLogin.Text;
			//	string password = passwordBox1.Text;

			//	var query = context.Исполнитель
			//			   .Where(p => p.Логин == login && p.Пароль == password)
			//			   .Select(c => c.Роль);

			//	foreach (var c in query)   // Только сейчас запрос начинает выполняться!
			//	{
			//		string position1 = String.Empty;
			//		position1 = Convert.ToString(c);

			//		if (position1 == "admin")

			//		{
			//			//this.Hide();
			//			About ab = new About();
			//			//ab.ShowDialog();
			//			//this.Close();
			//			ab.Show();
			//			Close();


			//		}
			//		else
			//		{
			//			if (position1 == "user")

			//			{
			//				//this.Hide();
			//				WindowStart mw = new WindowStart();
			//				//mw.ShowDialog();
			//				//this.Close();
			//				mw.Show();
			//				Close();
			//			}
			//		}
			//	}
			DateTime localDate = DateTime.Now;

			Исполнитель sotrudn = context.Исполнитель
				   .Where(v => v.Логин == textBoxLogin.Text && v.Пароль == passwordBox1.Text)
				   .AsEnumerable()
				   .FirstOrDefault();

			if (sotrudn is null)
			{
				lbLogin.Content = "неверное имя или пароль";
			}
			else
			{
				sRole = sotrudn.Роль;
				idSotrudn = sotrudn.КодИсполнителя;
				lbLogin.Content = sotrudn.ФИО + ", вы вошли в систему как " + sotrudn.Должность;

				if (sRole == "admin")
				{
					
					this.Hide();
					MessageBox.Show(sotrudn.ФИО + ", вы вошли в систему как " + sotrudn.Должность);
					WindowStart mw = new WindowStart();
					//mw.Owner = this;
					mw.ShowDialog();
					//mw.Show();
					//Close();
					this.Close();
					//ClickAdmin?.Invoke(this, EventArgs.Empty);
					
				}
				if (sRole == "user")
				{
					
					this.Hide();
					MessageBox.Show(sotrudn.ФИО + ", вы вошли в систему как " + sotrudn.Должность);
					WindowStart mw = new WindowStart();
					//ab.Owner = this;
					mw.ShowDialog();
					this.Close();
					//ab.Show();
					//Close();
					//ClickUser?.Invoke(this, EventArgs.Empty);
				}

			}

		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			//Environment.Exit(0);
			Close();
		}
	}
}

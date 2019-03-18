﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using System.Data;
using System.Data.Entity;
using Ninject;
using Smeta_DB;
using Word = Microsoft.Office.Interop.Word;
using System.IO;

namespace Smeta_1
{
    /// <summary>
    /// Interaction logic for Object.xaml
    /// </summary>
    public partial class Object : MetroWindow
	{
		private static int code;
		public static int WorkTypeCode;
		public static int Shifr;
		public static int WorkCode;
		public static int StavkaCode;
		public static int KofCode;
		public static double oldValue;

		private Word._Application oWord = new Word.Application();
        private object oMissing = System.Reflection.Missing.Value;

        public SmetaEntities SmetaContext { get; set; }

        public Object(SmetaEntities context)
		{
			InitializeComponent();
            SmetaContext = context;
            ReDrow();
			dgObject.Items.Clear();

			if (MainWindow.sRole == "admin")
			{
				menu_addProject.IsEnabled = true;
				menu_saveDogovor.IsEnabled = true;
				menu_Counter.IsEnabled = false;
				btnAddPrice.IsEnabled = false;
			}

			if (MainWindow.sRole == "user")
			{
				menu_addProject.IsEnabled = false;
				menu_saveDogovor.IsEnabled = false;
				menu_Counter.IsEnabled = true;
				btnAddPrice.IsEnabled = true;
			}
		}
		public void ReDrow()
		{
			listBox1.Items.Clear();
			foreach (var obj in SmetaContext.Объект.ToList())
			{
				listBox1.Items.Add(obj);
			}
		}

		private void listBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var obj = listBox1.SelectedItem as Объект;
			
			if (obj != null)
			{

				
				txtAd.Text = obj.Адрес;
				txtName.Text = obj.НаименованиеОбъекта;
				
				code = obj.Шифр;

				dgObject.ItemsSource = SmetaContext.Локальная_смета
                    .Where(sm => sm.Шифр == code)
                    .Select(l => new
                    {
                        l.КодРаботы,
	                    l.Справочник_расценок.ИмяРаботы,
	                    l.ФизОбъемРабот,
	                    l.ТрудоемкостьРаботы,
	                    l.СтоимостьРаботы
                    }).ToList();

				txSumTrud.Text = SmetaContext.Локальная_смета
                    .Where(c => c.Шифр == obj.Шифр)
                    .Select(c => c.ТрудоемкостьРаботы)
                    .Sum()
                    .ToString();

				txStoimost.Text = SmetaContext.Локальная_смета
                    .Where(c => c.Шифр == obj.Шифр)
                    .Select(c => c.СтоимостьРаботы)
                    .Sum()
                    .ToString();

				var dg = SmetaContext.Договор_подряда
                    .Where(d => d.Шифр == obj.Шифр)
                    .FirstOrDefault();
               
                txtData.Text = dg.ДатаДог.ToString();
                txtNomer.Text = dg.НомерДог.ToString();

                var cu = SmetaContext.Заказчик
                    .Where(c => c.КодЗаказчик == obj.КодЗаказчик)
                    .FirstOrDefault();
				
				txtCustName.Text = cu.НаименованиеЗаказчика;
				txtCustAdress.Text = cu.ЮрАдрес;
				txtCustYNP.Text = cu.УНП;
				txtCustPhone.Text = cu.Тел;
				txtCustMail.Text = cu.ЭлПочта;

				var pr = SmetaContext.Проектная_организация
                    .Where(p => p.КодПроектировщика == obj.КодПроектировщика)
                    .FirstOrDefault();
				txtProectName.Text = pr.НаименованиеПроектиров;
				txtProectAdress.Text = pr.ЮрАдрес;
				txtProectYNP.Text = pr.УНП;
				txtProectPhone.Text = pr.Тел;
				txtProectMail.Text = pr.ЭлПочта;
			}
		}

		private void dgObject_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
            
		}
		
		private void EditPriceToSmetaButton_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			
				EditPriceToSmeta b = new EditPriceToSmeta(SmetaContext);
				b.Owner = this;
				b.ShowDialog();
			
		}

		private void MenuItem_Click_OpenChart(object sender, RoutedEventArgs e)
		{
            var wa = new Chart(SmetaContext);
            wa.Owner = this;
            wa.ShowDialog();
        }

        private void MenuItem_Click_OpenDirectory(object sender, RoutedEventArgs e)
		{
            var wa = new Directory(SmetaContext);
            wa.Owner = this;
            wa.ShowDialog();
        }

		private void MenuItem_Click_OpenAbout(object sender, RoutedEventArgs e)
		{
			var wa = new About();
			wa.Owner = this;
			wa.ShowDialog();

		}

		private void MenuItem_Click_AddProjectCompany(object sender, RoutedEventArgs e)
		{
			var wa = new AddCustomer(SmetaContext);
			wa.Owner = this;
			wa.ShowDialog();
		}
		private void MenuItem_AddPriceToSmeta(object sender, RoutedEventArgs e)
		{
			var wa = new AddPriceToSmeta (SmetaContext);
			wa.Owner = this;
			wa.ShowDialog();
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			oWord.Quit(ref oMissing, ref oMissing, ref oMissing);
		}

		private void MenuItem_Click_SaveDogovor(object sender, RoutedEventArgs e)
		{
			var oDoc = LoadTemplate(Environment.CurrentDirectory + "\\template_dogovor.dotx");

            SetTemplate(oDoc);

            var date = $"{DateTime.Now.Day}.{DateTime.Now.Month}.{DateTime.Now.Year}";
            var time = DateTime.Now.ToLongTimeString().Replace(":", ".");
            var documentName = Environment.CurrentDirectory + $"\\Документ от {date} {time}.docx";
            
            try
            {
                SaveToDisk(oDoc, documentName);
                MessageBox.Show($"Документ создан под именем {documentName}");
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Save Error");
            }
            finally
            {
                oDoc.Close(ref oMissing, ref oMissing, ref oMissing);
            }
        }
		private Word._Document LoadTemplate(string filePath)
		{
			object oTemplate = filePath;
			Word._Document oDoc = oWord.Documents.Add(ref oTemplate, ref oMissing, ref oMissing, ref oMissing);
			return oDoc;
		}
		private void SetTemplate(Word._Document oDoc)
		{
			object oBookMark = "CustomerName";
			object oBookMark_1 = "DogNomer";
			object oBookMark_2 = "ProectName";
			object oBookMark_3 = "ObjectName";
			object oBookMark_4 = "ObjectAdress";
			object oBookMark_5 = "DogData";
			object oBookMark_6 = "WorkSum";
			object oBookMark_7 = "CustomerAdress";
			object oBookMark_8 = "CustomerYNP";
			object oBookMark_9 = "CustomerPhone";
			object oBookMark_10 = "ProectAdress";
			object oBookMark_11 = "ProectYNP";
			object oBookMark_12 = "ProectPhone";
			object oBookMark_13 = "ProectName_1";
			object oBookMark_14 = "CustomerName_1";
			oDoc.Bookmarks.get_Item(ref oBookMark).Range.Text = txtCustName.Text;
			oDoc.Bookmarks.get_Item(ref oBookMark_1).Range.Text = txtNomer.Text;
			oDoc.Bookmarks.get_Item(ref oBookMark_2).Range.Text = txtProectName.Text;
			oDoc.Bookmarks.get_Item(ref oBookMark_3).Range.Text = txtName.Text;
			oDoc.Bookmarks.get_Item(ref oBookMark_4).Range.Text = txtAd.Text;
			oDoc.Bookmarks.get_Item(ref oBookMark_5).Range.Text = txtData.Text;
			oDoc.Bookmarks.get_Item(ref oBookMark_6).Range.Text = txStoimost.Text;
			oDoc.Bookmarks.get_Item(ref oBookMark_7).Range.Text = txtCustAdress.Text;
			oDoc.Bookmarks.get_Item(ref oBookMark_8).Range.Text = txtCustYNP.Text;
			oDoc.Bookmarks.get_Item(ref oBookMark_9).Range.Text = txtCustPhone.Text;
			oDoc.Bookmarks.get_Item(ref oBookMark_10).Range.Text = txtProectAdress.Text;
			oDoc.Bookmarks.get_Item(ref oBookMark_11).Range.Text = txtProectYNP.Text;
			oDoc.Bookmarks.get_Item(ref oBookMark_12).Range.Text = txtProectPhone.Text;
			oDoc.Bookmarks.get_Item(ref oBookMark_13).Range.Text = txtProectName.Text;
			oDoc.Bookmarks.get_Item(ref oBookMark_14).Range.Text = txtCustName.Text;
		}
		private void SaveToDisk(Word._Document oDoc, string filePath)
		{
			object fileName = filePath;
            object fileExtension = Word.WdSaveFormat.wdFormatDocumentDefault;

            oDoc.SaveAs(
                ref fileName,
                ref fileExtension,
                ref oMissing,
                ref oMissing,
                ref oMissing,
                ref oMissing,
                ref oMissing,
                ref oMissing,
                ref oMissing,
                ref oMissing,
                ref oMissing,
                ref oMissing,
                ref oMissing,
                ref oMissing,
                ref oMissing,
                ref oMissing);
		}
		private void AddPriceToSmetaButton_Click(object sender, RoutedEventArgs e)
		{
			var wa = new AddPriceToSmeta(SmetaContext);
			wa.Owner = this;
			wa.ShowDialog();
		}


	}
}

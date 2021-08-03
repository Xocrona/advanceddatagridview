using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Zuby.ADGV;

namespace AdvancedDataGridViewSample
{
    public partial class FormMain : Form
    {
        private DataTable _dataTable = null;
        private DataSet _dataSet = null;

        private SortedDictionary<int, string> _filtersaved = new SortedDictionary<int, string>();
        private SortedDictionary<int, string> _sortsaved = new SortedDictionary<int, string>();

        private bool _testtranslations = false;
        private bool _testtranslationsFromFile = true;

        private static int DisplayItemsCounter = 100;

        private object[][] _inrows = new object[][] { };

        public FormMain()
        {
            InitializeComponent();

            //set localization strings
            Dictionary<string, string> translations = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> translation in AdvancedDataGridView.Translations)
            {
                if (!translations.ContainsKey(translation.Key))
                    translations.Add(translation.Key, "." + translation.Value);
            }
            foreach (KeyValuePair<string, string> translation in AdvancedDataGridViewSearchToolBar.Translations)
            {
                if (!translations.ContainsKey(translation.Key))
                    translations.Add(translation.Key, "." + translation.Value);
            }
            if (_testtranslations)
            {
                AdvancedDataGridView.SetTranslations(translations);
                AdvancedDataGridViewSearchToolBar.SetTranslations(translations);
            }
            if (_testtranslationsFromFile)
            {
                AdvancedDataGridView.SetTranslations(AdvancedDataGridView.LoadTranslationsFromFile("lang_es-ES.json"));
                AdvancedDataGridViewSearchToolBar.SetTranslations(AdvancedDataGridViewSearchToolBar.LoadTranslationsFromFile("lang_es-ES.json"));
            }

            //set filter and sort saved
            _filtersaved.Add(0, "");
            _sortsaved.Add(0, "");

            //initialize dataset
            _dataTable = new DataTable();
            _dataSet = new DataSet();

            //initialize bindingsource
            bindingSource_main.DataSource = _dataSet;

            //initialize datagridview
            advancedDataGridView_main.SetDoubleBuffered();
            advancedDataGridView_main.DataSource = bindingSource_main;

            //set bindingsource
            SetTestData();
        }

        public FormMain(object[][] inrows)
            : this()
        {
            _inrows = inrows;
        }

        private void SetTestData()
        {
            _dataTable = _dataSet.Tables.Add("TableTest");
            _dataTable.Columns.Add("int", typeof(int));
            _dataTable.Columns.Add("decimal", typeof(decimal));
            _dataTable.Columns.Add("double", typeof(double));
            _dataTable.Columns.Add("date", typeof(DateTime));
            _dataTable.Columns.Add("datetime", typeof(DateTime));
            _dataTable.Columns.Add("string", typeof(string));
            _dataTable.Columns.Add("boolean", typeof(bool));
            _dataTable.Columns.Add("guid", typeof(Guid));
            _dataTable.Columns.Add("image", typeof(Bitmap));
            _dataTable.Columns.Add("timespan", typeof(TimeSpan));

            bindingSource_main.DataMember = _dataTable.TableName;
        }

        private void AddTestData()
        {
            Random r = new Random();
            Image[] sampleimages = new Image[2];
            sampleimages[0] = Image.FromFile(Path.Combine(Application.StartupPath, "flag-green_24.png"));
            sampleimages[1] = Image.FromFile(Path.Combine(Application.StartupPath, "flag-red_24.png"));

            int maxMinutes = (int)((TimeSpan.FromHours(20) - TimeSpan.FromHours(10)).TotalMinutes);

            if (_inrows.Length == 0)
            {
                for (int i = 0; i < DisplayItemsCounter; i++)
                {
                    object[] newrow = new object[] {
                        i,
                        Math.Round((decimal)i*2/3, 6),
                        Math.Round(i % 2 == 0 ? (double)i*2/3 : (double)i/2, 6),
                        DateTime.Today.AddHours(i*2).AddHours(i%2 == 0 ?i*10+1:0).AddMinutes(i%2 == 0 ?i*10+1:0).AddSeconds(i%2 == 0 ?i*10+1:0).AddMilliseconds(i%2 == 0 ?i*10+1:0).Date,
                        DateTime.Today.AddHours(i*2).AddHours(i%2 == 0 ?i*10+1:0).AddMinutes(i%2 == 0 ?i*10+1:0).AddSeconds(i%2 == 0 ?i*10+1:0).AddMilliseconds(i%2 == 0 ?i*10+1:0),
                        i*2 % 3 == 0 ? null : i.ToString()+" str",
                        i % 2 == 0 ? true:false,
                        Guid.NewGuid(),
                        sampleimages[r.Next(0, 2)],
                        TimeSpan.FromHours(10).Add(TimeSpan.FromMinutes(r.Next(maxMinutes)))
                    };

                    _dataTable.Rows.Add(newrow);
                }
            }
            else
            {
                for (int i = 0; i < _inrows.Length; i++)
                {
                    _dataTable.Rows.Add(_inrows[i]);
                }
            }

        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            //add test data to bindsource
            AddTestData();

            //setup datagridview
            advancedDataGridView_main.SetFilterDateAndTimeEnabled(advancedDataGridView_main.Columns["datetime"], true);
            advancedDataGridView_main.SetSortEnabled(advancedDataGridView_main.Columns["guid"], false);
            advancedDataGridView_main.SetFilterChecklistEnabled(advancedDataGridView_main.Columns["guid"], false);
            advancedDataGridView_main.SortASC(advancedDataGridView_main.Columns["datetime"]);
            advancedDataGridView_main.SortDESC(advancedDataGridView_main.Columns["double"]);
            advancedDataGridView_main.SetTextFilterRemoveNodesOnSearch(advancedDataGridView_main.Columns["double"], false);
            advancedDataGridView_main.SetChecklistTextFilterRemoveNodesOnSearchMode(advancedDataGridView_main.Columns["decimal"], false);
            advancedDataGridView_main.SetFilterChecklistEnabled(advancedDataGridView_main.Columns["double"], false);
            advancedDataGridView_main.SetFilterCustomEnabled(advancedDataGridView_main.Columns["timespan"], false);
            advancedDataGridView_main.CleanSort(advancedDataGridView_main.Columns["datetime"]);
        }

        private void advancedDataGridView_main_FilterStringChanged(object sender, Zuby.ADGV.AdvancedDataGridView.FilterEventArgs e)
        {
            //eventually set the FilterString here
            //if e.Cancel is set to true one have to update the datasource here using
            //bindingSource_main.Filter = advancedDataGridView_main.FilterString;
            //otherwise it will be updated by the component
        }

        private void advancedDataGridView_main_SortStringChanged(object sender, Zuby.ADGV.AdvancedDataGridView.SortEventArgs e)
        {
            //eventually set the SortString here
            //if e.Cancel is set to true one have to update the datasource here
            //bindingSource_main.Sort = advancedDataGridView_main.SortString;
            //otherwise it will be updated by the component
        }

        private void bindingSource_main_ListChanged(object sender, ListChangedEventArgs e)
        {
            textBox_total.Text = bindingSource_main.List.Count.ToString();
        }

        private void button_unloadfilters_Click(object sender, EventArgs e)
        {
            advancedDataGridView_main.CleanFilterAndSort();
        }
    }
}

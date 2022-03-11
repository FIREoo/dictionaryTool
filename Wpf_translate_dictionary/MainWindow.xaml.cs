using System;
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
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Data;
using HtmlAgilityPack;

namespace Wpf_translate_dictionary
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private Thickness pose_of_tb_definition;

        private async void Btn_serach_Click(object sender, RoutedEventArgs e)
        {
            var web = new HtmlWeb();
            string searchWord = tb_input.Text;
            string url = "https://dictionary.cambridge.org/zht/%E8%A9%9E%E5%85%B8/%E8%8B%B1%E8%AA%9E-%E6%BC%A2%E8%AA%9E-%E7%B9%81%E9%AB%94/" + searchWord;
            var htmlDoc = web.Load(url);

            //var node = htmlDoc.DocumentNode.SelectSingleNode("//head/title");
            //string str = WebUtility.HtmlDecode(node.OuterHtml).ToString();
            //Console.WriteLine("fix Name: " + node.Name + "\n" + str);

            //查找的字
            //string search_word = htmlDoc.DocumentNode.SelectSingleNode("/html/body/div[2]/div/div[1]/div[2]/article/div[2]/div[4]/div/div/div/div[2]/div[1]/span/span").InnerText;
            //Console.WriteLine("search_word:" + search_word);

            //詞性
            string parts_of_speech = htmlDoc.DocumentNode.SelectSingleNode("/html/body/div[2]/div/div[1]/div[2]/article/div[2]/div[4]/div/div/div/div[2]/div[2]/span").InnerText;
            tb_parts_of_speech.Text = parts_of_speech + ".";
            //Console.WriteLine("parts_of_speech:" + parts_of_speech);

            //定義
            HtmlNodeCollection definition_Nodes = htmlDoc.DocumentNode.SelectSingleNode("/html/body/div[2]/div/div[1]/div[2]/article/div[2]/div[4]/div/div/div/div[3]/div[1]/div[2]").ChildNodes;
            //Console.WriteLine("definition count : " + (definition_Nodes.Count() - 2));//[0]?? [1]...[]  [last]更多範例
            tb_definition.Text = "";
            tb_definition.Margin = new Thickness(10, 0, 0, 0);

            for (int i = 0; i < definition_Nodes.Count() - 2; i++)
            {
                //Console.WriteLine("英:" + definition_Nodes[i].ChildNodes[3].ChildNodes[1].InnerText);
                //Console.WriteLine("中:" + definition_Nodes[i].ChildNodes[4].ChildNodes[1].InnerText);
                addToTextBlock(definition_Nodes[i].ChildNodes[3].ChildNodes[1].InnerText, Color.FromRgb(29, 42, 87));
                addToTextBlock(definition_Nodes[i].ChildNodes[4].ChildNodes[1].InnerText, Color.FromRgb(34, 134, 235));
                addToTextBlock("", Color.FromRgb(0, 0, 0));
                int example_sentence_count = definition_Nodes[i].ChildNodes[4].ChildNodes.Count() - 4;
                for (int index = 0; index < example_sentence_count; index++)
                {
                    int example_index = index + 3;
                    //Console.WriteLine($"example({index + 1}):" + definition_Nodes[i].ChildNodes[4].ChildNodes[example_index].ChildNodes[1].InnerText);
                    //Console.WriteLine($"example({index + 1}):" + definition_Nodes[i].ChildNodes[4].ChildNodes[example_index].ChildNodes[3].InnerText);
                    addToTextBlock(" - " + definition_Nodes[i].ChildNodes[4].ChildNodes[example_index].ChildNodes[1].InnerText, Color.FromRgb(29, 42, 87), 16);
                    addToTextBlock(" - " + definition_Nodes[i].ChildNodes[4].ChildNodes[example_index].ChildNodes[3].InnerText, Color.FromRgb(34, 134, 235), 16);
                }
                addToTextBlock("", Color.FromRgb(0, 0, 0));
            }
        }
        void addToTextBlock(string str, Color color, int size = 18, bool newLine = true)
        {
            tb_definition.Inlines.Add(new Run(str) { Foreground = new SolidColorBrush(color), FontSize = size });
            if (newLine)
                tb_definition.Inlines.Add(new Run("\r\n"));
        }

        private void Tb_input_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            IDataObject iData = Clipboard.GetDataObject();
            if (iData.GetDataPresent(DataFormats.Text))
            {
                tb_input.Text = (String)iData.GetData(DataFormats.Text);
            }
            tb_input.Text = Clipboard.GetText();
            e.Handled = true;
        }
        private void Tb_input_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
        private void Tb_input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Btn_serach_Click(sender, null);
            }
        }

        /// <summary>rolling word</summary>
        private void Tb_definition_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                if (tb_definition.Margin.Top < 0)
                {
                    Thickness thickness = new Thickness(tb_definition.Margin.Left, tb_definition.Margin.Top + 10, tb_definition.Margin.Right, tb_definition.Margin.Bottom);
                    //pose_of_tb_definition.Top = thickness.Top;
                    //pose_of_tb_definition.Bottom = pose_of_tb_definition.Top + tb_definition.ActualHeight;
                    tb_definition.Margin = thickness;
                }
            }
            else
            {
                if (tb_definition.ActualHeight > grid_definition.ActualHeight && (tb_definition.Margin.Top+ tb_definition.ActualHeight) > grid_definition.ActualHeight)
                {
                    Thickness thickness = new Thickness(tb_definition.Margin.Left, tb_definition.Margin.Top - 10, tb_definition.Margin.Right, tb_definition.Margin.Bottom);
                    //pose_of_tb_definition.Top = thickness.Top;
                    //pose_of_tb_definition.Bottom = pose_of_tb_definition.Top + tb_definition.ActualHeight;
                    tb_definition.Margin = thickness;
                }
            }
        }

        private void Btn_pop_out_Click(object sender, RoutedEventArgs e)
        {
            //if (this.WindowStyle == WindowStyle.ToolWindow)
            //{
            //    this.WindowStyle = WindowStyle.None;
            //}
            //if (this.WindowStyle == WindowStyle.None)
            //{
            //    this.WindowStyle = WindowStyle.ToolWindow;
            //}
            this.Left = 0;
            this.Top = 0;


        }

        private void Tb_definition_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TextBlock tb = (TextBlock)sender;
            pose_of_tb_definition.Top = tb.Margin.Top;
            pose_of_tb_definition.Left = tb.Margin.Left;
            pose_of_tb_definition.Bottom = tb.Margin.Top+tb.ActualHeight;
            pose_of_tb_definition.Right = tb.Margin.Left + tb.ActualWidth;
        }


    }
}

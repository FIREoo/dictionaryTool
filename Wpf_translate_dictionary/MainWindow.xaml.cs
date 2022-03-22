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

            tb_definition.Text = "";
            tb_definition.Margin = new Thickness(10, 0, 0, 0);

            //decode
            //var node = htmlDoc.DocumentNode.SelectSingleNode("//head/title");
            //string str = WebUtility.HtmlDecode(node.OuterHtml).ToString();
            //Console.WriteLine("fix Name: " + node.Name + "\n" + str);

            HtmlNodeCollection main_definition_nodes = new HtmlNodeCollection(null);
            HtmlNode dict_node = htmlDoc.DocumentNode.SelectSingleNode("/html/body/div[2]/div/div[1]/div[2]/article/div[2]");
            if(dict_node == null)
            {
                addToTextBlock("---N/A---", Color.FromRgb(0, 0, 0),style_Italic:true);
                return;
            }
            HtmlNode di_body_node = dict_node.SelectSingleNode("//div[@class='di-body']");

            HtmlNodeCollection entry_body_nodes = di_body_node.SelectNodes("//div[@class='entry']/div[@class='entry-body']/div[@class='pr entry-body__el']");//不知道為什麼 神奇的找到多個"entry-body" 裡的"pr entry-body__el"

            HtmlDocument entry_body_doc = new HtmlDocument();
            foreach (HtmlNode entry_body_node in entry_body_nodes)
            {
                entry_body_doc.LoadHtml(entry_body_node.InnerHtml);
                HtmlNode parts_of_speech_node = entry_body_doc.DocumentNode.SelectSingleNode("//div[@class='pos-header dpos-h']/div[@class='posgram dpos-g hdib lmr-5']/span[@class='pos dpos']");
                addToTextBlock("– " + parts_of_speech_node.InnerText, Color.FromRgb(29, 42, 87), size: 20, style_Italic: true, style_weight: true);
                HtmlNodeCollection sense_body_nodes = entry_body_doc.DocumentNode.SelectNodes("//div[@class='pos-body']/*/div[@class='sense-body dsense_b']");

                foreach (HtmlNode sense_body_node in sense_body_nodes)
                {
                    IList<HtmlNode> definition_one_block_nodes = findNodesClass(sense_body_node.ChildNodes, "def-block ddef_block ");

                    foreach (HtmlNode definition_one_block_node in definition_one_block_nodes)
                    {
                        try
                        {
                            HtmlNode eng_def_node = findSingleChild(definition_one_block_node, "//div[@class='ddef_h']/div[@class='def ddef_d db']");
                            //Console.WriteLine("英:" + eng_def_node.InnerText);
                            addToTextBlock(" ✦ " + eng_def_node.InnerText, Color.FromRgb(29, 42, 87));


                            string ch_trans = findSingleChild(definition_one_block_node, "//div[@class='def-body ddef_b']").ChildNodes[1].InnerText;
                            //Console.WriteLine("中:" + definition_more_node[0].ChildNodes[1].InnerText);
                            addToTextBlock("  " + ch_trans, Color.FromRgb(34, 134, 235));

                            if (flag_more_info)
                            {
                                addToTextBlock("", Color.FromRgb(0, 0, 0), 9);
                                HtmlNodeCollection example_nodes = findChilds(definition_one_block_node, "//div[@class='def-body ddef_b']/div[@class='examp dexamp']");
                                foreach (HtmlNode example_node in example_nodes) //trans dtrans dtrans-se hdb break-cj
                                {
                                    addToTextBlock(" - " + findSingleChild(example_node, "//span[@class='eg deg']").InnerText, Color.FromRgb(29, 42, 87), 16, style_Italic: true);
                                    addToTextBlock(" - " + findSingleChild(example_node, "//span[@class='trans dtrans dtrans-se hdb break-cj']").InnerText, Color.FromRgb(34, 134, 235), 16, style_Italic: true);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            return;
                            MessageBox.Show(ex.Message, "Error html decode");
                        }
                        addToTextBlock("", Color.FromRgb(0, 0, 0));
                    }


                }
                int a = sense_body_nodes.Count;
            }
        }
        private void Btn_serach_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string searchWord = tb_input.Text;
            string url = "https://dictionary.cambridge.org/zht/%E8%A9%9E%E5%85%B8/%E8%8B%B1%E8%AA%9E-%E6%BC%A2%E8%AA%9E-%E7%B9%81%E9%AB%94/" + searchWord;
            System.Diagnostics.Process.Start(url);
        }
        void addToTextBlock(string str, Color color, int size = 18, bool style_Italic = false, bool style_weight = false, bool newLine = true)
        {
            Run text = new Run(str);
            text.Foreground = new SolidColorBrush(color);
            text.FontSize = size;
            if (style_Italic) text.FontStyle = FontStyles.Italic;
            if (style_weight) text.FontWeight = FontWeights.DemiBold;
            tb_definition.Inlines.Add(text);

            if (newLine)
                tb_definition.Inlines.Add(new Run("\r\n"));
        }
        IList<HtmlNode> findNodesClass(IList<HtmlNode> nodes, string class_str)
        {
            IList<HtmlNode> clean_nodes = nodes.Where(node => node.Name == "div").ToList();
            return clean_nodes.Where(node => node.Attributes["class"].Value == class_str).ToList();
        }

        HtmlNode findSingleChild(HtmlNode node, string xpath)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(node.InnerHtml);
            return doc.DocumentNode.SelectSingleNode(xpath);
        }
        HtmlNodeCollection findChilds(HtmlNode node, string xpath)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(node.InnerHtml);
            return doc.DocumentNode.SelectNodes(xpath);
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
                if (tb_definition.ActualHeight > grid_definition.ActualHeight && (tb_definition.Margin.Top + tb_definition.ActualHeight) > grid_definition.ActualHeight)
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
            pose_of_tb_definition.Bottom = tb.Margin.Top + tb.ActualHeight;
            pose_of_tb_definition.Right = tb.Margin.Left + tb.ActualWidth;
        }


        bool flag_more_info = true;
        private void Cb_more_info_Click(object sender, RoutedEventArgs e)
        {
            flag_more_info = (((CheckBox)sender).IsChecked == true) ? true : false;
        }
    }
}

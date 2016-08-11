using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CurrencyRates.Models
{
    public static class Helper
    {
        public static string Translate(this HtmlHelper html, string expression)
        {
            switch (expression)
            {
                case "ABD DOLARI":
                    {
                        expression = "USA DOLLAR";
                        break;
                    }
                case "AVUSTRALYA DOLARI":
                    {
                        expression = "AUSTRALIAN DOLLAR";
                        break;
                    }
                case "BULGAR LEVASI":
                    {
                        expression = "BULGARIAN LEV";
                        break;
                    }
                case "ÇİN YUANI":
                    {
                        expression = "CHINESE RENMINBI";
                        break;
                    }
                case "DANİMARKA KRONU":
                    {
                        expression = "DANISH KRONE";
                        break;
                    }
                case "İSVİÇRE FRANGI":
                    {
                        expression = "SWISS FRANK";
                        break;
                    }
                case "İSVEÇ KRONU":
                    {
                        expression = "SWEDISH KRONA";
                        break;
                    }
                case "İNGİLİZ STERLİNİ":
                    {
                        expression = "GREAT BRITAIN POUND";
                        break;
                    }
                case "İRAN RİYALİ":
                    {
                        expression = "IRANIAN RIAL";
                        break;
                    }
                case "JAPON YENİ":
                    {
                        expression = "JAPENESE YEN";
                        break;
                    }
                case "KANADA DOLARI":
                    {
                        expression = "CANADIAN DOLLAR";
                        break;
                    }
                case "KUVEYT DİNARI":
                    {
                        expression = "KUWAITI DINAR";
                        break;
                    }
                case "NORVEÇ KRONU":
                    {
                        expression = "NORWEGIAN KRONE";
                        break;
                    }
                case "PAKİSTAN RUPİSİ":
                    {
                        expression = "PAKISTANI RUPEE";
                        break;
                    }
                case "RUMEN LEYİ":
                    {
                        expression = "NEW LEU";
                        break;
                    }
                case "RUS RUBLESİ":
                    {
                        expression = "RUSSIAN ROUBLE";
                        break;
                    }
                case "SUUDİ ARABİSTAN RİYALİ":
                    {
                        expression = "SAUDI RIYAL";
                        break;
                    }
            }
            return expression;
        }
    }
}

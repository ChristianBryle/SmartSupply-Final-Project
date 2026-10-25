using System;
using System.Data;
using System.IO;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace SmartSupply
{
    internal static class ReceiptPdf
    {
        private const string FolderName = "Reciept";

        public static string GetReceiptFolder()
        {
            DirectoryInfo dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (dir != null)
            {
                string candidate = Path.Combine(dir.FullName, FolderName);
                if (Directory.Exists(candidate))
                    return candidate;
                dir = dir.Parent;
            }

            string fallback = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FolderName);
            Directory.CreateDirectory(fallback);
            return fallback;
        }

        public static string Save(string saleNumber, DateTime saleDate, DataTable items, decimal total, decimal cash, decimal change)
        {
            string path = Path.Combine(GetReceiptFolder(), $"{saleNumber}_{saleDate:yyyyMMdd_HHmmss}.pdf");

            using (PdfDocument document = new PdfDocument())
            {
                document.Info.Title = "Receipt " + saleNumber;
                PdfPage page = document.AddPage();
                page.Size = PageSize.A5;

                XPdfFontOptions options = new XPdfFontOptions(PdfFontEncoding.Unicode);
                XFont title = new XFont("Arial", 16, XFontStyle.Bold, options);
                XFont bold = new XFont("Arial", 10, XFontStyle.Bold, options);
                XFont normal = new XFont("Arial", 10, XFontStyle.Regular, options);

                XGraphics gfx = XGraphics.FromPdfPage(page);
                try
                {
                    double left = 30, right = page.Width.Point - 30, y = 40;
                    double width = right - left;

                    gfx.DrawString("SmartSupply", title, XBrushes.Black, new XRect(left, y, width, 20), XStringFormats.TopCenter);
                    y += 26;
                    gfx.DrawString("Official Receipt", normal, XBrushes.Black, new XRect(left, y, width, 14), XStringFormats.TopCenter);
                    y += 24;
                    gfx.DrawString("Receipt No: " + saleNumber, normal, XBrushes.Black, left, y);
                    y += 14;
                    gfx.DrawString("Date: " + saleDate.ToString("yyyy-MM-dd HH:mm:ss"), normal, XBrushes.Black, left, y);
                    y += 12;
                    gfx.DrawLine(XPens.Black, left, y, right, y);
                    y += 16;

                    double colQty = left + width * 0.55, colPrice = left + width * 0.75;
                    gfx.DrawString("Product", bold, XBrushes.Black, left, y);
                    gfx.DrawString("Qty", bold, XBrushes.Black, colQty, y);
                    gfx.DrawString("Price", bold, XBrushes.Black, colPrice, y);
                    DrawRight(gfx, "Amount", bold, right, y);
                    y += 6;
                    gfx.DrawLine(XPens.Black, left, y, right, y);
                    y += 14;

                    foreach (DataRow row in items.Rows)
                    {
                        if (y > page.Height.Point - 100)
                        {
                            gfx.Dispose();
                            page = document.AddPage();
                            page.Size = PageSize.A5;
                            gfx = XGraphics.FromPdfPage(page);
                            y = 40;
                        }

                        int qty = row.Field<int>("Qty");
                        decimal price = row.Field<decimal>("Price");
                        gfx.DrawString(Truncate(row.Field<string>("Product"), 28), normal, XBrushes.Black, left, y);
                        gfx.DrawString(qty.ToString(), normal, XBrushes.Black, colQty, y);
                        gfx.DrawString(price.ToString("N2"), normal, XBrushes.Black, colPrice, y);
                        DrawRight(gfx, (qty * price).ToString("N2"), normal, right, y);
                        y += 16;
                    }

                    gfx.DrawLine(XPens.Black, left, y - 8, right, y - 8);
                    y += 8;
                    DrawTotalLine(gfx, "Total:", total, bold, left, right, ref y);
                    DrawTotalLine(gfx, "Cash:", cash, normal, left, right, ref y);
                    DrawTotalLine(gfx, "Change:", change, normal, left, right, ref y);
                    y += 16;
                    gfx.DrawString("Thank you for your purchase!", normal, XBrushes.Black, new XRect(left, y, width, 14), XStringFormats.TopCenter);
                }
                finally
                {
                    gfx.Dispose();
                }

                document.Save(path);
            }

            return path;
        }

        private static void DrawTotalLine(XGraphics gfx, string label, decimal value, XFont font, double left, double right, ref double y)
        {
            gfx.DrawString(label, font, XBrushes.Black, left, y);
            DrawRight(gfx, "\u20B1 " + value.ToString("N2"), font, right, y);
            y += 16;
        }

        private static void DrawRight(XGraphics gfx, string text, XFont font, double right, double y)
        {
            double w = gfx.MeasureString(text, font).Width;
            gfx.DrawString(text, font, XBrushes.Black, right - w, y);
        }

        private static string Truncate(string value, int max)
        {
            return value.Length <= max ? value : value.Substring(0, max - 1) + "…";
        }
    }
}
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace SmartSupply
{
    internal static class QuotationPdf
    {
        private const string FolderPath = @"C:\Users\chris\source\repos\SmartSupply\SmartSupply\Download Quotations";

        public static string GetQuotationFolder()
        {
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }
            return FolderPath;
        }

        public static string Save(int quotationId, string quotationNumber, string quotationName, DateTime quotationDate, string createdBy)
        {
            // Get quotation items from database
            DataTable items = DBconnection.Query(
                "SELECT p.product_name AS [Product], i.quantity AS [Qty], i.unit_price AS [Price], i.total_price AS [Total] " +
                "FROM quotation_items i JOIN products p ON p.product_id = i.product_id " +
                "WHERE i.quotation_id = @id ORDER BY p.product_name",
                new SqlParameter("@id", quotationId));

            decimal grandTotal = 0;
            foreach (DataRow row in items.Rows)
            {
                grandTotal += row.Field<decimal>("Total");
            }

            string fileName = $"{quotationNumber}_{quotationName}.pdf";
            string path = Path.Combine(GetQuotationFolder(), fileName);

            using (PdfDocument document = new PdfDocument())
            {
                document.Info.Title = "Quotation " + quotationNumber;
                PdfPage page = document.AddPage();
                page.Orientation = PageOrientation.Portrait;
                page.Size = PageSize.A4;

                XPdfFontOptions options = new XPdfFontOptions(PdfFontEncoding.Unicode);
                XFont title = new XFont("Arial", 16, XFontStyle.Bold, options);
                XFont bold = new XFont("Arial", 11, XFontStyle.Bold, options);
                XFont normal = new XFont("Arial", 10, XFontStyle.Regular, options);

                XGraphics gfx = XGraphics.FromPdfPage(page);
                try
                {
                    double left = 40, right = page.Width.Point - 40, y = 40;
                    double width = right - left;

                    // Header
                    gfx.DrawString("SmartSupply", title, XBrushes.Black, new XRect(left, y, width, 20), XStringFormats.TopCenter);
                    y += 26;
                    gfx.DrawString("Quotation", bold, XBrushes.Black, new XRect(left, y, width, 16), XStringFormats.TopCenter);
                    y += 24;

                    // Quotation Details
                    gfx.DrawString("Quotation No: " + quotationNumber, normal, XBrushes.Black, left, y);
                    y += 16;
                    gfx.DrawString("Quotation Name: " + quotationName, normal, XBrushes.Black, left, y);
                    y += 16;
                    gfx.DrawString("Date: " + quotationDate.ToString("yyyy-MM-dd"), normal, XBrushes.Black, left, y);
                    y += 16;
                    gfx.DrawString("Created By: " + createdBy, normal, XBrushes.Black, left, y);
                    y += 12;

                    gfx.DrawLine(XPens.Black, left, y, right, y);
                    y += 16;

                    // Column Headers
                    double colQty = left + width * 0.45, colPrice = left + width * 0.65;
                    gfx.DrawString("Product", bold, XBrushes.Black, left, y);
                    gfx.DrawString("Qty", bold, XBrushes.Black, colQty, y);
                    gfx.DrawString("Unit Price", bold, XBrushes.Black, colPrice, y);
                    DrawRight(gfx, "Total", bold, right, y);
                    y += 6;
                    gfx.DrawLine(XPens.Black, left, y, right, y);
                    y += 14;

                    // Items
                    foreach (DataRow row in items.Rows)
                    {
                        if (y > page.Height.Point - 100)
                        {
                            gfx.Dispose();
                            page = document.AddPage();
                            page.Size = PageSize.A4;
                            gfx = XGraphics.FromPdfPage(page);
                            y = 40;
                        }

                        int qty = row.Field<int>("Qty");
                        decimal price = row.Field<decimal>("Price");
                        decimal total = row.Field<decimal>("Total");
                        string product = Truncate(row.Field<string>("Product"), 40);

                        gfx.DrawString(product, normal, XBrushes.Black, left, y);
                        gfx.DrawString(qty.ToString(), normal, XBrushes.Black, colQty, y);
                        gfx.DrawString(price.ToString("N2"), normal, XBrushes.Black, colPrice, y);
                        DrawRight(gfx, total.ToString("N2"), normal, right, y);
                        y += 16;
                    }

                    // Footer
                    gfx.DrawLine(XPens.Black, left, y - 8, right, y - 8);
                    y += 8;
                    DrawTotalLine(gfx, "Grand Total:", grandTotal, bold, left, right, ref y);
                    y += 16;
                    gfx.DrawString("Thank you for your interest!", normal, XBrushes.Black, new XRect(left, y, width, 14), XStringFormats.TopCenter);
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
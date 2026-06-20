using KX_Project.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace KX_Project.Services.Pdf
{
    public class InvoiceDocument : IDocument
    {
        private readonly Order _order;

        public InvoiceDocument(Order order)
        {
            _order = order;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Margin(50);
                    page.Size(PageSizes.A4);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(ComposeContent);
                    page.Footer().Element(ComposeFooter);
                });
        }

        void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("KORA STORE").FontSize(20).SemiBold().FontColor(Colors.Blue.Darken2);
                    column.Item().Text("Thời trang & Phụ kiện cao cấp").FontSize(10).FontColor(Colors.Grey.Medium);
                });

                row.ConstantItem(100).AlignRight().Column(column =>
                {
                    column.Item().Text($"HÓA ĐƠN").FontSize(20).Bold();
                    column.Item().Text($"#ORD-{_order.Id:D5}").FontSize(12);
                    column.Item().Text($"{_order.OrderDate:dd/MM/yyyy}");
                });
            });
        }

        void ComposeContent(IContainer container)
        {
            container.PaddingVertical(20).Column(column =>
            {
                column.Item().Text("THÔNG TIN KHÁCH HÀNG:").SemiBold();
                column.Item().PaddingBottom(5);
                column.Item().Text($"Khách hàng: {_order.User?.FullName ?? "Khách"}");
                column.Item().Text($"Điện thoại: {_order.User?.PhoneNumber ?? "N/A"}");
                column.Item().Text($"Địa chỉ giao hàng: {_order.ShippingAddress}");
                column.Item().PaddingBottom(20);

                column.Item().Element(ComposeTable);

                var totalPrice = _order.OrderDetails?.Sum(x => x.Price * x.Quantity) ?? 0;
                column.Item().PaddingTop(20).AlignRight().Text($"Tổng thanh toán: {totalPrice:C0}").FontSize(14).SemiBold();
            });
        }

        void ComposeTable(IContainer container)
        {
            container.Table(table =>
            {
                // step 1
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(30);
                    columns.RelativeColumn(3);
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                // step 2
                table.Header(header =>
                {
                    header.Cell().Text("#").SemiBold();
                    header.Cell().Text("Sản phẩm").SemiBold();
                    header.Cell().AlignRight().Text("Đơn giá").SemiBold();
                    header.Cell().AlignRight().Text("SL").SemiBold();
                    header.Cell().AlignRight().Text("Thành tiền").SemiBold();
                    
                    header.Cell().ColumnSpan(5).PaddingTop(5).BorderBottom(1).BorderColor(Colors.Black);
                });

                // step 3
                if (_order.OrderDetails != null)
                {
                    int index = 1;
                    foreach (var item in _order.OrderDetails)
                    {
                        var productName = item.Product?.Name ?? "Sản phẩm không xác định";
                        table.Cell().Text(index.ToString());
                        table.Cell().Text(productName);
                        table.Cell().AlignRight().Text($"{item.Price:C0}");
                        table.Cell().AlignRight().Text(item.Quantity.ToString());
                        table.Cell().AlignRight().Text($"{item.Price * item.Quantity:C0}");
                        index++;
                    }
                }
            });
        }

        void ComposeFooter(IContainer container)
        {
            container.AlignCenter().Text(x =>
            {
                x.Span("Cảm ơn quý khách đã mua sắm tại KoraStore!").FontSize(10);
            });
        }
    }
}

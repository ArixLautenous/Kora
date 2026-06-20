using KX_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KX_Project.Services;
using KX_Project.Services.Pdf;
using QuestPDF.Fluent;
using ClosedXML.Excel;

namespace KX_Project.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public OrderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == user.Id)
                .ToListAsync();

            if (!cartItems.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            ViewBag.CartItems = cartItems;
            ViewBag.TotalAmount = cartItems.Sum(i => i.Product.Price * i.Quantity);

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(Order model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == user.Id)
                .ToListAsync();

            if (!cartItems.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            if (ModelState.IsValid)
            {
                model.UserId = user.Id;
                model.OrderDate = DateTime.Now;
                model.TotalAmount = cartItems.Sum(i => i.Product.Price * i.Quantity);
                model.Status = "Pending";

                _context.Orders.Add(model);
                await _context.SaveChangesAsync();

                foreach (var item in cartItems)
                {
                    var orderDetail = new OrderDetail
                    {
                        OrderId = model.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Product.Price
                    };
                    _context.OrderDetails.Add(orderDetail);
                }

                _context.CartItems.RemoveRange(cartItems);
                await _context.SaveChangesAsync();

                // Gửi email xác nhận
                if (!string.IsNullOrEmpty(user.Email))
                {
                    string subject = $"KoraStore - Xác nhận đơn hàng #{model.Id:D5}";
                    string body = $"<h3>Cảm ơn {user.FullName ?? user.UserName} đã đặt hàng!</h3>" +
                                  $"<p>Đơn hàng <b>#{model.Id:D5}</b> của bạn đã được ghi nhận.</p>" +
                                  $"<p>Tổng tiền: <b>{model.TotalAmount:C0}</b></p>" +
                                  $"<p>Chúng tôi sẽ sớm liên hệ để giao hàng đến địa chỉ: {model.ShippingAddress}</p>";
                    await _emailSender.SendEmailAsync(user.Email, subject, body);
                }

                return RedirectToAction("OrderSuccess", new { id = model.Id });
            }

            ViewBag.CartItems = cartItems;
            ViewBag.TotalAmount = cartItems.Sum(i => i.Product.Price * i.Quantity);
            return View(model);
        }

        public IActionResult OrderSuccess(int id)
        {
            ViewBag.OrderId = id;
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> ManageOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
            return View(orders);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.Status = status;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật trạng thái đơn hàng thành công!";
            }
            return RedirectToAction(nameof(ManageOrders));
        }

        [HttpGet]
        public async Task<IActionResult> DownloadInvoice(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails!)
                .ThenInclude(od => od.Product)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            // Chỉ Admin, Staff hoặc chính người đặt mới được tải hóa đơn
            var user = await _userManager.GetUserAsync(User);
            if (!User.IsInRole("Admin") && !User.IsInRole("Staff") && order.UserId != user?.Id)
            {
                return Forbid();
            }

            var document = new InvoiceDocument(order);
            var pdfBytes = document.GeneratePdf();

            return File(pdfBytes, "application/pdf", $"HoaDon_KoraStore_{order.Id:D5}.pdf");
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> ExportOrdersToExcel()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("DanhSachDonHang");

            // Header
            worksheet.Cell(1, 1).Value = "Mã Đơn";
            worksheet.Cell(1, 2).Value = "Khách Hàng";
            worksheet.Cell(1, 3).Value = "Điện Thoại";
            worksheet.Cell(1, 4).Value = "Ngày Đặt";
            worksheet.Cell(1, 5).Value = "Tổng Tiền";
            worksheet.Cell(1, 6).Value = "Trạng Thái";

            var headerRange = worksheet.Range("A1:F1");
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

            // Dữ liệu
            int row = 2;
            foreach (var order in orders)
            {
                worksheet.Cell(row, 1).Value = $"ORD-{order.Id:D5}";
                worksheet.Cell(row, 2).Value = order.User?.FullName ?? "Khách";
                worksheet.Cell(row, 3).Value = order.User?.PhoneNumber ?? "";
                worksheet.Cell(row, 4).Value = order.OrderDate.ToString("dd/MM/yyyy HH:mm");
                worksheet.Cell(row, 5).Value = order.TotalAmount;
                worksheet.Cell(row, 5).Style.NumberFormat.Format = "#,##0";
                worksheet.Cell(row, 6).Value = order.Status;
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"BaoCao_DonHang_{DateTime.Now:yyyyMMdd}.xlsx");
        }
    }
}

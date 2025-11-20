using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using M_Suite.Data;
using M_Suite.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;

namespace M_Suite.Controllers
{
    public class UserController : Controller
    {
        private readonly MSuiteContext _context;

        public UserController(MSuiteContext context)
        {
            _context = context;
        }


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Login(string username, string password, bool remember = false)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Username and password are required";
                return View();
            }

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UsLogin == username && u.UsDeleted == 0);
                
                if (user == null)
                {
                    ViewBag.Error = "Invalid username or password";
                    return View();
                }

                // Check if user is active
                if (user.UsActive != 1)
                {
                    ViewBag.Error = "Your account is inactive. Please contact administrator.";
                    return View();
                }

                // Check if account has expired
                if (user.UsExpiryDate.HasValue && user.UsExpiryDate.Value < DateTime.Now)
                {
                    ViewBag.Error = "Your account has expired. Please contact administrator.";
                    return View();
                }

                // Verify password - support both plain text (for migration) and BCrypt hashed passwords
                bool passwordValid = false;
                if (user.UsPassword.StartsWith("$2a$") || user.UsPassword.StartsWith("$2b$") || user.UsPassword.StartsWith("$2y$"))
                {
                    // BCrypt hash
                    passwordValid = BCrypt.Net.BCrypt.Verify(password, user.UsPassword);
                }
                else
                {
                    // Plain text (for backward compatibility during migration)
                    passwordValid = user.UsPassword == password;
                    // If plain text matches, hash it for future use
                    if (passwordValid)
                    {
                        user.UsPassword = BCrypt.Net.BCrypt.HashPassword(password);
                        await _context.SaveChangesAsync();
                    }
                }

                if (!passwordValid)
                {
                    ViewBag.Error = "Invalid username or password";
                    return View();
                }

                // Store user-related data for authentication and authorization
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UsLogin),
                    new Claim(ClaimTypes.NameIdentifier, user.UsId.ToString()),
                    new Claim("FullName", $"{user.UsFirstName} {user.UsLastName}"),
                    new Claim(ClaimTypes.Role, user.UsType ?? string.Empty)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = remember,
                    ExpiresUtc = remember ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(8),
                    AllowRefresh = true
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "An error occurred during login. Please try again.";
                // Log error in production
                return View();
            }
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "User");

        }

        // GET: User
        [Authorize]
        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            int pageSize = 10;
            var query = _context.Users
                .Where(u => u.UsDeleted == 0)
                .Include(u => u.UsUs)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(n =>
                    n.UsLogin.Contains(searchString) ||
                    n.UsFirstName.Contains(searchString) ||
                    n.UsLastName.Contains(searchString) ||
                    n.UsEmail.Contains(searchString)
                );
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var users = await query
                .OrderBy(u => u.UsLogin)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchString = searchString;

            return View(users);
        }


        // GET: User/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.UsUs)
                .FirstOrDefaultAsync(m => m.UsId == id && m.UsDeleted == 0);
            
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: User/Create
        [Authorize]
        public IActionResult Create()
        {
            ViewData["Title"] = "Create";
            ViewData["UsUsId"] = new SelectList(_context.Users.Where(u => u.UsDeleted == 0), "UsId", "UsLogin");
            return View();
        }

        // POST: User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("UsId,UsUsId,UsThpId,UsCdIdGen,UsCdIdTtl,UsCode,UsFirstName,UsLastName,UsShortName,UsLogin,UsPassword,UsEmail,UsReceiveNotification,UsExpiryDate,UsActive,UsDeleted,UsDbUser,UsImported,UsReadonly,UsToken,UsRoute,UsPrefix,UsType")] User user)
        {
            // Check if username already exists
            if (await _context.Users.AnyAsync(u => u.UsLogin == user.UsLogin && u.UsDeleted == 0))
            {
                ModelState.AddModelError("UsLogin", "Username already exists");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Hash password before saving
                    if (!string.IsNullOrWhiteSpace(user.UsPassword))
                    {
                        user.UsPassword = BCrypt.Net.BCrypt.HashPassword(user.UsPassword);
                    }

                    // Set default values
                    if (user.UsDeleted == 0 && !user.UsDeleted.HasValue)
                    {
                        user.UsDeleted = 0;
                    }
                    if (user.UsActive == null)
                    {
                        user.UsActive = 1;
                    }

                    _context.Add(user);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "User created successfully";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while creating the user: " + ex.Message);
                }
            }
            ViewData["UsUsId"] = new SelectList(_context.Users.Where(u => u.UsDeleted == 0), "UsId", "UsLogin", user.UsUsId);
            return View(user);
        }

        // GET: User/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null || user.UsDeleted == 1)
            {
                return NotFound();
            }
            
            // Store original password hash to avoid re-hashing if password wasn't changed
            ViewData["OriginalPassword"] = user.UsPassword;
            ViewData["UsUsId"] = new SelectList(_context.Users.Where(u => u.UsDeleted == 0 && u.UsId != id), "UsId", "UsLogin", user.UsUsId);
            return View(user);
        }

        // POST: User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("UsId,UsUsId,UsThpId,UsCdIdGen,UsCdIdTtl,UsCode,UsFirstName,UsLastName,UsShortName,UsLogin,UsPassword,UsEmail,UsReceiveNotification,UsExpiryDate,UsActive,UsDeleted,UsDbUser,UsImported,UsReadonly,UsToken,UsRoute,UsPrefix,UsType")] User user, string? originalPassword)
        {
            if (id != user.UsId)
            {
                return NotFound();
            }

            // Check if username already exists (excluding current user)
            if (await _context.Users.AnyAsync(u => u.UsLogin == user.UsLogin && u.UsId != id && u.UsDeleted == 0))
            {
                ModelState.AddModelError("UsLogin", "Username already exists");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = await _context.Users.FindAsync(id);
                    if (existingUser == null)
                    {
                        return NotFound();
                    }

                    // Only hash password if it was changed (not a BCrypt hash already)
                    if (!string.IsNullOrWhiteSpace(user.UsPassword))
                    {
                        // If password is not already hashed, hash it
                        if (!user.UsPassword.StartsWith("$2a$") && !user.UsPassword.StartsWith("$2b$") && !user.UsPassword.StartsWith("$2y$"))
                        {
                            user.UsPassword = BCrypt.Net.BCrypt.HashPassword(user.UsPassword);
                        }
                    }
                    else
                    {
                        // Keep original password if not changed
                        user.UsPassword = existingUser.UsPassword;
                    }

                    _context.Update(user);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "User updated successfully";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.UsId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while updating the user: " + ex.Message);
                }
            }
            ViewData["UsUsId"] = new SelectList(_context.Users.Where(u => u.UsDeleted == 0 && u.UsId != id), "UsId", "UsLogin", user.UsUsId);
            return View(user);
        }

        // GET: User/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.UsUs)
                .FirstOrDefaultAsync(m => m.UsId == id && m.UsDeleted == 0);
            
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                // Soft delete instead of hard delete
                user.UsDeleted = 1;
                user.UsActive = 0;
                _context.Update(user);
                await _context.SaveChangesAsync();
                TempData["Success"] = "User deleted successfully";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UsId == id);
        }
    }
}

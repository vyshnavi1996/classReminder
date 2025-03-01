﻿using Event_Management.Models;
using Event_Management.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Event_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class EventsController : Controller
    {

        private readonly EventService _eventService;
        private readonly IWebHostEnvironment _hostEnviroment;
        public EventsController(EventService eventService, IWebHostEnvironment hostEnviroment)
        {
            _eventService = eventService;
            _hostEnviroment = hostEnviroment;
        }

        [HttpGet]
        [Route("Index")]
        public IActionResult Index()
        {
            List<EventModel> list = new List<EventModel>();
            if (HttpContext.Session.GetString("UserId") != null)
            {
                var userId = HttpContext.Session.GetString("UserId");
                list = _eventService.SearchList(userId);
            }
            return View(list);
        }

        [HttpGet]
        [Route("Create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Route("Create/events")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventsCreateViewModel events)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var obj = new EventModel();
                    string wwwRootPath = _hostEnviroment.WebRootPath;
                    string[] days = { };

                    if (events.ImageFile != null)
                    {
                        string imgFileName = Path.GetFileNameWithoutExtension(events.ImageFile.FileName);
                        string extension = Path.GetExtension(events.ImageFile.FileName);
                        imgFileName = imgFileName + DateTime.Now.ToString("yyMMssfff") + extension;
                        string path = Path.Combine(wwwRootPath, "Image", imgFileName);

                        using (var fileStream = new FileStream(path, FileMode.Create))
                        {
                            await events.ImageFile.CopyToAsync(fileStream);
                        }
                        obj.ImageName = imgFileName;
                    }
                    else
                    {
                        obj.ImageName = "";
                    }




                    obj.EventName = events.EventName;
                    obj.Location = events.Location;
                    obj.UserID = HttpContext.Session.GetString("UserId");
                    if (events.Recarsive == true)
                    {
                        List<string> list = new List<string>();
                        if (events.M)
                        {
                            list.Add(DayOfWeek.Monday.ToString());
                        }
                        if (events.T)
                        {
                            list.Add(DayOfWeek.Tuesday.ToString());
                        }
                        if (events.W)
                        {
                            list.Add(DayOfWeek.Wednesday.ToString());
                        }
                        if (events.TH)
                        {
                            list.Add(DayOfWeek.Thursday.ToString());
                        }
                        if (events.F)
                        {
                            list.Add(DayOfWeek.Friday.ToString());
                        }
                        if (list.Count > 0)
                        {
                            obj.Days = list.ToArray();
                        }
                        else
                        {
                            obj.Days = days;
                        }
                        obj.StartDate = events.StartDate;
                        obj.EndDate = events.EndDate;
                        obj.IsRecarsive = true;
                        obj.Date = null;
                    }
                    else
                    {
                        obj.IsRecarsive = false;
                        obj.Date = events.Date;
                        obj.StartDate = null;
                        obj.EndDate = null;
                    }

                    _eventService.Create(obj);
                    ViewBag.Message = "Event Created Successfully";
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "Error.! " + ex.Message;
                }


            }

            return RedirectToAction("Index", "Events");
        }

        [HttpPut]
        [Route("Edit/Id")]
        public IActionResult Edit(string Id)
        {
            var evd = _eventService.Get(Id);
            var ev = new EventsCreateViewModel();

            if (evd != null)
            {
                ev.Id = evd.Id;
                ev.EventName = evd.EventName;
                ev.Location = evd.Location;
                ev.Date = evd.Date;
                ev.StartDate = evd.StartDate;
                ev.EndDate = evd.EndDate;
                ev.ImageName = evd.ImageName;
                if (evd.IsRecarsive == true)
                {
                    ev.Recarsive = true;

                    foreach (var i in evd.Days)
                    {
                        if (i == DayOfWeek.Monday.ToString())
                        {
                            ev.M = true;
                        }
                        else if (i == DayOfWeek.Tuesday.ToString())
                        {
                            ev.T = true;
                        }
                        else if (i == DayOfWeek.Wednesday.ToString())
                        {
                            ev.W = true;
                        }
                        else if (i == DayOfWeek.Thursday.ToString())
                        {
                            ev.TH = true;
                        }
                        else if (i == DayOfWeek.Friday.ToString())
                        {
                            ev.F = true;
                        }
                    }
                }

            }

            return View(ev);
        }

        [HttpPost]
        [Route("Edit/events")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EventsCreateViewModel events)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (events != null)
                    {
                        string wwwRootPath = _hostEnviroment.WebRootPath;
                        string[] days = Array.Empty<string>();

                        var ev = _eventService.Get(events.Id);

                        if (ev.ImageName != null && ev.ImageName != "default")
                        {
                            var oldImg = Path.Combine(wwwRootPath, "Image", ev.ImageName);
                            if (System.IO.File.Exists(oldImg))
                            {
                                System.IO.File.Delete(oldImg);
                            }
                        }

                        if (events.ImageFile != null)
                        {
                            string imgFileName = Path.GetFileNameWithoutExtension(events.ImageFile.FileName);
                            string extension = Path.GetExtension(events.ImageFile.FileName);
                            imgFileName = imgFileName + DateTime.Now.ToString("yyMMssfff") + extension;
                            string path = Path.Combine(wwwRootPath, "Image", imgFileName);

                            using (var fileStream = new FileStream(path, FileMode.Create))
                            {
                                await events.ImageFile.CopyToAsync(fileStream);
                            }
                            ev.ImageName = imgFileName;
                        }
                        else
                        {
                            ev.ImageName = "";
                        }


                        ev.EventName = events.EventName;
                        ev.Location = events.Location;
                        if (events.Recarsive == true)
                        {
                            List<string> list = new List<string>();
                            if (events.M == true)
                            {
                                list.Add(DayOfWeek.Monday.ToString());
                            }
                            if (events.T)
                            {
                                list.Add(DayOfWeek.Tuesday.ToString());
                            }
                            if (events.W)
                            {
                                list.Add(DayOfWeek.Wednesday.ToString());
                            }
                            if (events.TH)
                            {
                                list.Add(DayOfWeek.Thursday.ToString());
                            }
                            if (events.F)
                            {
                                list.Add(DayOfWeek.Friday.ToString());
                            }
                            if (list.Count > 0)
                            {
                                ev.Days = list.ToArray();
                            }
                            else
                            {
                                ev.Days = days;
                            }
                            ev.StartDate = events.StartDate;
                            ev.EndDate = events.EndDate;
                            ev.IsRecarsive = true;
                            ev.Date = null;
                        }
                        else
                        {
                            ev.IsRecarsive = false;
                            ev.Date = events.Date;
                            ev.StartDate = null;
                            ev.EndDate = null;
                        }
                        _eventService.Update(events.Id, ev);
                        ViewBag.Message = "Event Successfully Updated.!";
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "Error.! " + ex.Message;
                }
            }
            return RedirectToAction("Index", "Events");
        }

        [HttpDelete]
        [Route("Delete/id")]
        public IActionResult Delete(string id)
        {
            var item = _eventService.Get(id);

            return View(item);
        }

        [HttpPost]
        [Route("DeleteConfirm/id")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirm(string id)
        {
            string wwwRootPath = _hostEnviroment.WebRootPath;

            var ev = _eventService.Get(id);

            if (ev.ImageName != null && ev.ImageName != "default")
            {
                var oldImg = Path.Combine(wwwRootPath, "Image", ev.ImageName);
                if (System.IO.File.Exists(oldImg))
                {
                    System.IO.File.Delete(oldImg);
                }
            }

            _eventService.Remove(id);

            return RedirectToAction("Index", "Events");
        }

    }
}

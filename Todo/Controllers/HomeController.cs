﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Todo.Models;
using Todo.Models.ViewModels;

namespace Todo.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        var todoListViewModel = GetAllTodos(); //naudoja metodą taskams atvaizduot
        return View(todoListViewModel);
    }

    [HttpGet]
    public JsonResult PopulateForm(int id)
    {
        var todo = GetById(id);
        return Json(todo);
    }

    internal TodoViewModel GetAllTodos() //get metodas visam užduočių listui, TodoViewModel ateina iš View Models klasės
    {
        List<TodoItem> todoList = new();

        using (SqliteConnection con =
            new SqliteConnection("Data Source=db.sqlite"))
        {
            using (var tableCmd = con.CreateCommand())
            {
                con.Open();
                tableCmd.CommandText = "SELECT * FROM todo";

                using (var reader = tableCmd.ExecuteReader()) 
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            todoList.Add(  //pildo Todo listą
                                new TodoItem
                                {
                                    Id = reader.GetInt32(0),
                                    Name = reader.GetString(1) 
                                });
                        }
                    }
                    else //jei listas tuščias, grąžins naują tuščią modelį
                    {
                        return new TodoViewModel
                        {
                            TodoList = todoList
                        };
                    }
                };
            }
        }
        return new TodoViewModel
        {
            TodoList = todoList
        };
    }

    internal TodoItem GetById(int id) //vidinis get metodas, naudojamas užduočių atnaujinimui
    {
       TodoItem todo = new();

        using (var connection =
            new SqliteConnection("Data Source=db.sqlite"))
        {
            using (var tableCmd = connection.CreateCommand())
            {
                connection.Open();
                tableCmd.CommandText = $"SELECT * FROM todo WHERE Id = '{id}'";

                using (var reader = tableCmd.ExecuteReader()) 
                {
                    if (reader.HasRows)
                    {
                        reader.Read();
                        todo.Id = reader.GetInt32(0);
                        todo.Name = reader.GetString(1);                    
                    }
                    else //jei listas tuščias, grąžins naują tuščią modelį
                    {
                        return todo;
                    }
                };
            }
        }
        return todo;
    }
    public RedirectResult Insert(TodoItem todo) //insert to db metodas, grąžina puslapio redirektą
    {
        
        using (SqliteConnection con =
                new SqliteConnection("Data Source=db.sqlite"))
        {
            using (var tableCmd = con.CreateCommand())
            {
                con.Open();
                tableCmd.CommandText = $"INSERT INTO todo (name) VALUES ('{todo.Name}')";
                try
                {
                    tableCmd.ExecuteNonQuery(); //ExecuteNonQuery used for executing queries that does not return any data
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
        return Redirect("http://localhost:5134/");
    }

    public RedirectResult Update(TodoItem todo) //insert to db metodas, grąžina puslapio redirektą
    {
        
        using (SqliteConnection con =
                new SqliteConnection("Data Source=db.sqlite"))
        {
            using (var tableCmd = con.CreateCommand())
            {
                con.Open();
                tableCmd.CommandText = $"UPDATE todo SET name ='{todo.Name}' WHERE Id = '{todo.Id}'";
                try
                {
                    tableCmd.ExecuteNonQuery(); //ExecuteNonQuery used for executing queries that does not return any data
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
        return Redirect("http://localhost:5134/");
    }

    [HttpPost] //iš MVC namespace, padaro, kad šitą funkciją galima būtų iškviesti per browserį (per ajaxą)
    public JsonResult Delete(int id)
    {
        using (SqliteConnection con =
            new SqliteConnection("Data Source=db.sqlite"))
            {
                using (var tableCmd = con.CreateCommand())
                {
                    con.Open();
                    tableCmd.CommandText = $"DELETE from todo WHERE Id ='{id}'";
                    tableCmd.ExecuteNonQuery();
                }
            }

            return Json(new {});
    }
}

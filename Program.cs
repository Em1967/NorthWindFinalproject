// See https://aka.ms/new-console-template for more information

﻿using NLog;
string path = Directory.GetCurrentDirectory() + "//nlog.config";

// create instance of Logger
var logger = LogManager.Setup().LoadConfigurationFromFile(path).GetCurrentClassLogger();

logger.Info("Program started");

Console.WriteLine("Hello World!");

logger.Info("Program ended");

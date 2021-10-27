global using System;
global using System.Collections.Generic;
global using System.Data;
global using System.Data.Common;
global using System.IO;
global using System.Linq;
global using System.Reflection;
global using System.Text;
global using System.Text.RegularExpressions;
global using System.Text.Json;
global using System.Threading.Tasks;

global using Microsoft.Data.SqlClient;
global using Microsoft.Extensions.Caching.Memory;
global using Microsoft.Extensions.DependencyInjection;

global using Dapper;

global using PopForums.Configuration;
global using PopForums.Email;
global using PopForums.ExternalLogin;
global using PopForums.Models;
global using PopForums.Repositories;
global using PopForums.ScoringGame;
global using PopForums.Services;

global using PopForums.Sql.Repositories;
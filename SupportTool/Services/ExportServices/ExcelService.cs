using ClosedXML.Excel;
using SupportTool.Services.ActiveDirectoryServices;
using System;
using System.Collections.Generic;
using System.Data;
using System.DirectoryServices;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportTool.Services.ExportServices
{
	public class ExcelService
	{
		public static string ExcelFileFilter = "Excel file (*.xlsx)|*.xlsx";

		public static IObservable<Unit> SaveUsersToExcelFile(IEnumerable<string> users, string path) => Observable.Start(() =>
		{
			SaveUsersToExcelFile(users.Select(x => ActiveDirectoryService.Current.SearchDirectory(x).Take(1).Wait()), path);
		});

		public static IObservable<Unit> SaveUsersToExcelFile(IEnumerable<DirectoryEntry> users, string path) => Observable.Start(() =>
		{
			var table = new DataTable("Sheet 1")
			{
				Columns =
				{
					new DataColumn("ID", typeof(string)),
					new DataColumn("Given Name", typeof(string)),
					new DataColumn("Surname", typeof(string)),
					new DataColumn("Email", typeof(string)),
					new DataColumn("Title", typeof(string)),
					new DataColumn("HF", typeof(string))
				}
			};

			foreach (var user in users)
			{
				table.Rows.Add(
					user.Properties["samaccountname"].Value?.ToString() ?? "",
					user.Properties["givenname"].Value?.ToString() ?? "",
					user.Properties["sn"].Value?.ToString() ?? "",
					user.Properties["mail"].Value?.ToString() ?? "",
					user.Properties["title"].Value?.ToString() ?? "",
					user.Properties["company"].Value?.ToString() ?? "");
			}

			var workBook = new XLWorkbook();
			workBook.Worksheets.Add(table);
			workBook.SaveAs(path);
		});

		public static IObservable<Unit> SaveGroupsToExcelFile(IEnumerable<string> groups, string path) => Observable.Start(() =>
		{
			SaveGroupsToExcelFile(groups.Select(x => ActiveDirectoryService.Current.SearchDirectory(x).Take(1).Wait()), path);
		});

		public static IObservable<Unit> SaveGroupsToExcelFile(IEnumerable<DirectoryEntry> groups, string path) => Observable.Start(() =>
		{
			var table = new DataTable("Sheet 1")
			{
				Columns =
				{
					new DataColumn("Name", typeof(string)),
					new DataColumn("Description", typeof(string)),
					new DataColumn("Notes", typeof(string))
				}
			};

			foreach (var group in groups)
			{
				table.Rows.Add(
					(group.Properties["cn"].Value?.ToString() ?? ""),
					(group.Properties["description"].Value?.ToString() ?? ""),
					(group.Properties["info"].Value?.ToString() ?? ""));
			}

			var workBook = new XLWorkbook();
			workBook.Worksheets.Add(table);
			workBook.SaveAs(path);
		});
	}
}

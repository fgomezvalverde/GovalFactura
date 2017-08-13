FlexCel Studio is a library to read and write xls and xlsx files, export them to html or pdf and print and preview them. 
All code is written in C#, and full sources are included.

It is available for Xamarin.Mac, Xamarin.iOS, Xamarin.Android, Windows Phone, Windows Store (WinRT), Mono for Linux and Desktop .NET. A single license is valid for all supported platforms.

At its core, FlexCel has three main components:

## 1. An xls/x engine
This is the most lower level component in the pack. It contains an API to read and write xls or xlsx files,
and the main object you use for it is the XlsFile object.

####Example:

```csharp
public void CreateFile()
{
   XlsFile xls = new XlsFile(1, true);
   xls.SetCellValue(1, 1, "FlexCel says Hello!");
   xls.Save("result.xlsx");
}

```

There is a tool available for Windows and OSX that you can use to find out how to use the xls/x engine.
Just create the file you want to create in Excel, and open the file with:

  * [APIMate for OSX](http://www.tmssoftware.biz/flexcel/tools/net/ApiMate.dmg)
  * [APIMate for Windows](http://www.tmssoftware.biz/flexcel/tools/net/ApiMate.zip)
  
APIMate will show you the needed code (in C# or VB.NET) to create the file. Full source for APIMate is included, so you can study how it works too.

## 2. A reporting engine 
This is a higher level component for creating xls or xlsx files. When using it, you create a file in Excel which will be
used as a template where the reporting engine will fill the values. Internally, this component uses the xls/x engine to create the files,
but it allows you to do it in a more declarative way. It allows you to use Excel as the report designer.

####Example:
In Excel, create a file and write:

<pre>
|   |         A          |           B              |        C                          |
|---|--------------------|--------------------------|-----------------------------------|
| 1 | <#Customer.Name>   | <#if(<#Customer.Vip>;VIP;)> | <#Customer.Age>                   |
| 2 |                    |                          |                                   |
| 3 |                    |                          | ="Average Age: " & Average(C1:C2) |
</pre>

Add a named range (in the ribbon-> Formula tab->Name Manager). Name it "__Customer__", and
make it go from A1 to C1. This name must have 2 underscores at the beginning and end, and the
text between the underscores must be the same as the text between <#.... .> tags. It tells 
FlexCel which rows are used for each record in the database.

Save the file as "template.xlsx"

Then write the following code:

```csharp
class Customer
{  
   public string Name { get; set; }
   public bool Vip { get; set; }
   public int Age {get; set; }
}

public void CreateFile()
{
   List<Customer> Customers = GetCustomers();
   
   using (FlexCelReport fr = new FlexCelReport(true))
   {
      fr.AddTable("Customer", Customers);
      fr.Run("template.xlsx", "result.xlsx");
   }
}
```

This is a basic report, but you can do a lot more, like multiple level master-detail reports, cross ref
reports, etc. Take a look at the Windows examples for a list of possible reports.

## 3. A rendering engine
This component is used to convert any xls or xlsx file to pdf, images, html or to print them. Internally it
also uses the xls/x engine, as the reporting engine does.

####Example:
To convert a file to pdf:

```csharp
	XlsFile xls = new XlsFile(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "result.xlsx"));

	using (var pdf = new FlexCelPdfExport(xls, true))
	{
		pdf.Export(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "result.pdf"));
	}
```

To convert a file to html:

```csharp
    XlsFile xls = new XlsFile(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "result.xlsx"));

    using (var html = new FlexCelHtmlExport(xls, true))
    {
        html.Export(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "result.html"), null);
    }
```

using System;
using System.IO;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SkiaSharp;


namespace CatWorx.BadgeMaker
{
    class Util
    {
        public static void PrintEmployees(List<Employee> employees)
        {
            for (int i = 0; i < employees.Count; i++)
            {
                string template = "{0,-10}\t{1,-20}\t{2}";
                Console.WriteLine(String.Format(template, employees[i].GetId(), employees[i].GetFullName(), employees[i].GetPhotoUrl()));
            }
        }

        public static void MakeCSV(List<Employee> employees)
        {
            //Check to see if the folder exsists
            if (!Directory.Exists("data"))
            {
                //If not, create it
                Directory.CreateDirectory("data");
            }

            using (StreamWriter file = new StreamWriter("data/employees.csv"))
            {
                file.WriteLine("ID,Name,PhotoUrl");

                for (int i = 0; i < employees.Count; i++)
                {
                    string template = "{0},{1},{2}";
                    file.WriteLine(String.Format(template, employees[i].GetId(), employees[i].GetFullName(), employees[i].GetPhotoUrl()));
                }

            }
        }

        async public static Task MakeBadges(List<Employee> employees)
        {
            int BADGE_WIDTH = 669;
            int BADGE_HEIGHT = 1044;

            int PHOTO_LEFT_X = 184;
            int PHOTO_TOP_Y = 215;
            int PHOTO_RIGHT_X = 486;
            int PHOTO_BOTTOM_Y = 517;

            int COMPANY_NAME_Y = 150;

            int EMPLOYEE_NAME_Y = 600;

            int EMPLOYEE_ID_Y = 730;

            using (HttpClient client = new HttpClient())
            {   //Instance of HTTPClient is disposed after code in the block has run
                for (int i = 0; i < employees.Count; i++)
                {
                    //GetStreamAsync() method and convert the stream it returns into an SKImage
                    SKImage photo = SKImage.FromEncodedData(await client.GetStreamAsync(employees[i].GetPhotoUrl()));
                    SKImage background = SKImage.FromEncodedData(File.OpenRead("badge.png"));

                    //SKBitemap data type to initialize the size parameters
                    //SKCanvas class to insert images on the badge
                    SKBitmap badge = new SKBitmap(BADGE_WIDTH, BADGE_HEIGHT);
                    SKCanvas canvas = new SKCanvas(badge);

                    //DrawImage() to let us draw image on the badge and SKRect allows us to allocate a position and size on the badge
                    canvas.DrawImage(background, new SKRect(0, 0, BADGE_WIDTH, BADGE_HEIGHT));
                    canvas.DrawImage(photo, new SKRect(PHOTO_LEFT_X, PHOTO_TOP_Y, PHOTO_RIGHT_X, PHOTO_BOTTOM_Y));

                    //Characterize the text with SKPaint object
                    SKPaint paint = new SKPaint();
                    paint.TextSize = 42.0f;
                    paint.IsAntialias = true;
                    paint.Color = SKColors.White;
                    paint.IsStroke = false;
                    paint.TextAlign = SKTextAlign.Center;
                    paint.Typeface = SKTypeface.FromFamilyName("Arial");

                    //Draw the Compnay Name with DrawText() method and use the vatiable paint to add chracteristics
                    canvas.DrawText(employees[i].GetCompanyName(), BADGE_WIDTH / 2f, COMPANY_NAME_Y, paint);

                    //Color for name on badge
                    paint.Color = SKColors.Black;
                    //Draw name text on badge
                    canvas.DrawText(employees[i].GetFullName(), BADGE_WIDTH / 2f,  EMPLOYEE_NAME_Y, paint);

                    //Typeface for employee id
                    paint.Typeface = SKTypeface.FromFamilyName("Courier New");
                    //Draw id emplyoee id number
                    canvas.DrawText(employees[i].GetId().ToString(), BADGE_WIDTH / 2f, EMPLOYEE_ID_Y, paint);

                    SKImage finaleImage = SKImage.FromBitmap(badge);
                    SKData data = finaleImage.Encode();
                    
                    //use string interpolination ti insert the employee id number into the file name to defferentiate each badge image file
                    string template = "data/{0}_badge.png";
                    data.SaveTo(File.OpenWrite(string.Format(template, employees[i].GetId())));
                }
            }
        }
    }
}
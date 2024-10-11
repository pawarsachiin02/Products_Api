using Products_Api.Model;

namespace Products_Api.Global
{
    public static class Converter
    {
        public static List<Product> highLighter(List<Product> filteredSizeData,string[] highlightArray )
        {
            try
            {
                // looping for multiple highlight items

                for (int i = 0; i < highlightArray.Length; i++)
                {
                    for (int j = 0; j < filteredSizeData.Count(); j++)
                    {
                        //Checking for highlight color exists or not
                        if (filteredSizeData[j].description.Contains(highlightArray[i].ToString()))
                        {
                            //Assigning <em></em> according to highlight color
                            filteredSizeData[j].description = filteredSizeData[j].description.Replace("green", "<em>green</em>").Replace("blue", "<em>blue</em>").Replace("red", "<em>red</em>");                           
                        }
                    }
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }

            return filteredSizeData;
        }
    }
}

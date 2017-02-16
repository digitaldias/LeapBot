using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LEAPBot.Dialogs
{
    public class Hotel
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string ImageUrl { get; set; }

        public string HomepageUrl { get; set; }

        public static List<Hotel> Recommended = new List<Hotel>
        {
            new Hotel {
                Name        = "Radisson Blu Scandinavia Hotel, Oslo",
                Description = "This inspiring hotel in the city center rises 22 stories high, offering beautiful views of both the capital and the nearby fjord. Both business and leisure travellers will find the Radisson Blu a convenient base from which to explore Norway’s culture and a wide variety of attractions. Right outside the entrance are excellent public transport links; and the Royal Palace, National Gallery and Opera House are all within walking distance. Oslo’s main street, Karl Johans Gate, is minutes away as well as the Nationaltheateret metro and train station.",
                ImageUrl    = "https://leap.blob.core.windows.net/other/radisson_blu_scandinavia_hotel_oslo_photo40_oslo_norway.jpg",
                HomepageUrl = "https://www.radissonblu.com/en/scandinaviahotel-oslo"
            },

            new Hotel {
                Name        = "Radisson Blu Plaza hotel, Oslo",
                Description = " This hotel stands within walking distance of top attractions such as the Royal Palace, National Gallery and Oslo Spektrum Arena. Browse the shops of nearby Karl Johans Gate, or access train, bus, tram and metro lines at Oslo Central Station, next to the hotel.",
                ImageUrl    = "https://leap.blob.core.windows.net/other/radisson-blu-plaza-hotel-oslo_240820091104016369.jpg",
                HomepageUrl = "https://www.radissonblu.com/en/plazahotel-oslo"
            }
        };
    }


}
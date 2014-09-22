using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAnalytic.Domain.Abstract;
using DataAnalytic.Domain.Entities;

namespace DataAnalytic.Domain.Concrete
{
    public class EFVideoRepository : IObjectRepository<Video>
    {
        private EFDbContext context = new EFDbContext();
        public IQueryable<Video> GetDataSet
        {
            get
            {
                return context.Videos;
            }
        }

        public int AddObject(Video video)
        {
            Video duplicateVideo = context.Videos.FirstOrDefault(ovd => ovd.URL == video.URL);
            if (duplicateVideo == null)
            {
                context.Videos.Add(video);
                context.SaveChanges();
                return video.ID;
            }
            return -1;
        }

        public void UpdateDate(string URL)
        {
            Video video = context.Videos.FirstOrDefault(ovd => ovd.URL == URL);
            if (video != null)
            {
                video.UpdatedDate = DateTime.Now;
                context.SaveChanges();
            }
        }

        public Video DeleteEntity(int dataSourceID)
        {
            throw new NotImplementedException();
        }

        public void SaveObject(Video entity)
        {
            Video video = context.Videos.FirstOrDefault(ovd => ovd.ID == entity.ID);
            if (video != null)
            {
                video.URL = entity.URL;
                video.TvChannel = entity.TvChannel;
                video.Title = entity.Title;
                video.Description = entity.Description;
                video.Category = entity.Category;
                video.Tags = entity.Tags;
                video.IsAvailable = entity.IsAvailable;
                video.YouTubeId = entity.YouTubeId;
                video.UpdatedDate = DateTime.Now;
                context.SaveChanges();
            }
        }


        public Video Find(int id)
        {
            return context.Videos.FirstOrDefault<Video>(vd => vd.ID == id);
        }
    }
}

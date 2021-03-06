﻿using RssAlphaReader.Model.Interface;
using RssAlphaReader.Model.SubContent.Metadata.MediaRssContext.SubContent;
using System.Collections.Generic;

namespace RssAlphaReader.Model.SubContent.Metadata.MediaRssContext
{
    public class MediaRssExtension : IModel
    {
        public RssCategory Category { get; set; }
        public MediaRssCopyright Copyright { get; set; }
        public MediaRssCredit Credit { get; set; }
        public RssText Description { get; set; }
        public List<MediaRssContent> Group { get; set; }
        public MediaRssHash Hash { get; set; }
        public string Keywords { get; set; }
        public MediaRssPlayer Player { get; set; }
        public MediaRssRating Rating { get; set; }
        public MediaRssText Text { get; set; }
        public MediaRssThumbnails Thumbnails { get; set; }
        public RssText Title { get; set; }
    }
}

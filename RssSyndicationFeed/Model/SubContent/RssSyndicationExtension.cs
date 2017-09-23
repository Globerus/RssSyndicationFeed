﻿using RssSyndicationFeed.Model.Interface;

namespace RssSyndicationFeed.Model.SubContent
{
    public class RssSyndicationExtension
    {
        public IContext Context { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string Namespace { get; set; }
    }
}

namespace Sitecore.Support.LayoutService.Presentation.Pipelines.RenderRendering
{
  using Sitecore;
  using Sitecore.Caching;
  using Sitecore.Common;
  using Sitecore.Diagnostics;
  using Sitecore.LayoutService.ItemRendering;
  using Sitecore.LayoutService.Presentation;
  using Sitecore.LayoutService.Presentation.Pipelines.RenderRendering;
  using Sitecore.Mvc.Pipelines.Response.RenderRendering;
  using Sitecore.Sites;
  using System;
  using System.IO;

  public class RenderJsonFromCache : RenderRenderingProcessor
  {
    public override void Process(RenderRenderingArgs args)
    {
      Assert.ArgumentNotNull(args, "args");
      if (Switcher<JsonRenderingContext, JsonRenderingContext>.CurrentValue != null && Switcher<JsonRenderingContext, JsonRenderingContext>.CurrentValue.RenderedRenderingContext != null && !args.Rendered)
      {
        string cacheKey = args.CacheKey;
        if (args.Cacheable && !string.IsNullOrEmpty(cacheKey) && Render(cacheKey, args.Writer, args, Switcher<JsonRenderingContext, JsonRenderingContext>.CurrentValue.RenderedRenderingContext))
        {
          args.Rendered = true;
          args.Cacheable = false;
          args.UsedCache = true;
        }
      }
    }

    protected virtual bool Render(string cacheKey, TextWriter writer, RenderRenderingArgs args, RenderedJsonRenderingContext renderedRenderingContext)
    {
      SiteContext site = Context.Site;
      if (site == null)
      {
        return false;
      }
      HtmlCache htmlCache = CacheManager.GetHtmlCache(site);
      if (htmlCache == null)
      {
        return false;
      }
      string html = htmlCache.GetHtml(cacheKey);
      if (html == null)
      {
        return false;
      }
      Guid? renderingIdFromStub = renderedRenderingContext.GetRenderingIdFromStub(html);
      if (!renderingIdFromStub.HasValue)
      {
        return false;
      }
      RenderedJsonRendering renderedJsonRendering = htmlCache.InnerCache.GetValue(AddRecordedJsonToCache.JsonCacheKey(cacheKey)) as RenderedJsonRendering;
      if (renderedJsonRendering == null)
      {
        return false;
      }
      renderedRenderingContext.AddRenderedRendering(renderedJsonRendering, renderingIdFromStub.Value);
      writer.Write(html);
      return true;
    }
  }
}
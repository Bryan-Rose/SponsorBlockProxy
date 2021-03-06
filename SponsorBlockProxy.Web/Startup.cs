using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

using SponsorBlockProxy.Audio.FP;
using SponsorBlockProxy.Audio.Splice;
using SponsorBlockProxy.RSS;
using SponsorBlockProxy.Models;

namespace SponsorBlockProxy.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient(typeof(Loggy<>));
            services.AddTransient<RSSProxyService>();
            services.AddSingleton<FPService>();
            services.AddSingleton<SplicerService>();

            var section = Configuration.GetSection("AppSettings");
            var config = section.Get<AppSettingsConfig>();
            services.AddSingleton(config);

            if (config.MediaTool == AppSettingsConfig.MediaToolEnum.FFMPEG)
            {
                services.AddSingleton<ICutter, FFmpegCutter>();
            }
            else if (config.MediaTool == AppSettingsConfig.MediaToolEnum.MP3SPLT)
            {
                services.AddSingleton<ICutter, Mp3SpltCutter>();
            }

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "test", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "test v1"));
            }

            //app.UseHttpsRedirection();

            app.UseRouting();

            //app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

        }
    }
}

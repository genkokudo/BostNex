using BostNex.Data;
using BostNex.Services;
using BostNexShared.Entity;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace BostNex.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LightController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly string _parameterName = "Lighters";
        private readonly IAesService _aes;

        public LightController(ApplicationDbContext context, IAesService aes)
        {
            _context = context;
            _aes = aes;
        }

        // GET: api/Light
        /// <summary>
        /// 今まで脱出した人数を返す
        /// データが無ければ作成する
        /// パラメータなし:現在の値を取得
        /// </summary>
        /// <returns>今までの脱出者数</returns>
        [HttpGet]
        public async Task<ActionResult<string>> Get()
        {
            // まずDBからデータを準備
            var data = await GetLightDataAsync();

            return data!.Value!;
        }

        // POST: api/Light
        [HttpPost]
        public async Task<ActionResult<string>> Post(Rootobject value)
        {
            // 復号して時刻形式だったらOK
            var date = _aes.Decrypt(value.Date);
            if (IsDate(date))
            {
                // まずDBからデータを準備
                var data = await GetLightDataAsync();

                // カウントアップしてDBを更新する
                var dbDataInt = int.Parse(data.Value!);
                dbDataInt++;
                data.Value = dbDataInt.ToString();
                await _context.SaveChangesAsync();

                return data!.Value!;
            }
            return "";
        }

        public class Rootobject
        {
            public string Date { get; set; } = "";
        }

        private bool IsDate(string dt)
        {
            string fmt = "yyyy/MM/dd HH:mm:ss";
            DateTimeStyles dts = DateTimeStyles.None;
            DateTime outValue;

            return DateTime.TryParseExact(dt, fmt, null, dts, out outValue);
        }

        /// <summary>
        /// 脱出データがあれば返す
        /// 無ければ作成して返す
        /// </summary>
        /// <returns></returns>
        private async Task<General> GetLightDataAsync()
        {
            // まずDBからデータを準備
            var dbData = _context.Generals.FirstOrDefault(x => x.Name == _parameterName);
            if (dbData is null)
            {
                // データが無ければ作成する
                dbData = new General { Name = _parameterName, Value = "0" };
                _context.Generals.Add(dbData);
                await _context.SaveChangesAsync();
            }
            return dbData;
        }

        //// GET: api/Light/5
        //[HttpGet("{id}")]
        //public async Task<ActionResult<string>> GetGeneral(string id)
        //{
        //    // まずDBからデータを準備
        //    var data = await GetLightDataAsync();

        //    // カウントアップしてDBを更新する
        //    var dbDataInt = int.Parse(data.Value!);
        //    dbDataInt++;
        //    data.Value = dbDataInt.ToString();
        //    await _context.SaveChangesAsync();

        //    return data!.Value!;
        //}



    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BostNex.Data;
using BostNexShared.Entity;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Xml.Linq;

namespace BostNex.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EscapeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly string _parameterName = "Escapers";

        public EscapeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Escape
        /// <summary>
        /// 今まで脱出した人数を返す
        /// データが無ければ作成する
        /// パラメータなし:現在の値を取得
        /// パラメータあり:現在の値に1を加えて取得、パラメータは使用しないが"name"を付けること。
        /// </summary>
        /// <returns>今までの脱出者数</returns>
        [HttpGet]
        public async Task<ActionResult<string>> Get()
        {
            // まずDBからデータを準備
            var data = await GetEscapeDataAsync();
            
            return data!.Value!;
        }

        /// <summary>
        /// 脱出データがあれば返す
        /// 無ければ作成して返す
        /// </summary>
        /// <returns></returns>
        private async Task<General> GetEscapeDataAsync()
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

        // GET: api/Generals/5
        [HttpGet("{id}")]
        public async Task<ActionResult<string>> GetGeneral(string id)
        {
            // まずDBからデータを準備
            var data = await GetEscapeDataAsync();

            // カウントアップしてDBを更新する
            var dbDataInt = int.Parse(data.Value!);
            dbDataInt++;
            data.Value = dbDataInt.ToString();
            await _context.SaveChangesAsync();
            
            return data!.Value!;
        }


        //// POST: api/Generals
        //[HttpPost]
        //public async Task<ActionResult<General>> PostGeneral(General general)
        //{
        //  if (_context.Generals == null)
        //  {
        //      return Problem("Entity set 'ApplicationDbContext.Generals'  is null.");
        //  }
        //    _context.Generals.Add(general);
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction("GetGeneral", new { id = general.Id }, general);
        //}

    }
}

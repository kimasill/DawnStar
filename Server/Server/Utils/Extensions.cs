using Microsoft.EntityFrameworkCore;
using Server.DB;
using SharedDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Utils
{
    public static class Extensions
    {
        public static bool SaveChangesEx(this AppDbContext db)
        {
            try
            {
                db.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public static bool SaveChangesEx(this SharedDbContext db)
        {
            try
            {
                db.SaveChanges();
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // 동시성 예외 처리
                foreach (var entry in ex.Entries)
                {
                    if (entry.Entity is ShopDb)
                    {
                        var proposedValues = entry.CurrentValues;
                        var databaseValues = entry.GetDatabaseValues();

                        if (databaseValues == null)
                        {
                            // 데이터베이스에 엔터티가 존재하지 않음
                            entry.State = EntityState.Detached;
                        }
                        else
                        {
                            // 데이터베이스 값으로 현재 값을 덮어씌움
                            entry.OriginalValues.SetValues(databaseValues);
                        }
                    }
                    else
                    {
                        throw new NotSupportedException("동시성 예외는 ShopDb 엔터티에서만 지원됩니다.");
                    }
                }

                // 재시도
                db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static async Task<bool> SaveChangesExAsync(this AppDbContext db)
        {
            try
            {
                await db.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // 동시성 예외 처리
                foreach (var entry in ex.Entries)
                {
                    if (entry.Entity is PlayerDb)
                    {
                        var proposedValues = entry.CurrentValues;
                        var databaseValues = await entry.GetDatabaseValuesAsync();

                        if (databaseValues == null)
                        {
                            // 데이터베이스에 엔터티가 존재하지 않음
                            entry.State = EntityState.Detached;
                        }
                        else
                        {
                            // 데이터베이스 값으로 현재 값을 덮어씌움
                            entry.OriginalValues.SetValues(databaseValues);
                        }
                    }
                    else
                    {
                        throw new NotSupportedException("동시성 예외는 PlayerDb 엔터티에서만 지원됩니다.");
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }
    }
}

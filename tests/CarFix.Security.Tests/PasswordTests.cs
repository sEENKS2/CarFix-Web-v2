using Entidades.Core;
using Controladora;
using System.Text.Json;
using Xunit;

namespace CarFix.Security.Tests;

public class PasswordTests
{
    private static string RandomPassword() => Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(24));

    [Fact]
    public void SamePasswordHasIndependentSaltsAndRejectsWrongPassword()
    {
        var password = RandomPassword();
        var first = PasswordSecurity.Hash(password);
        var second = PasswordSecurity.Hash(password);
        Assert.NotEqual(first, second);
        Assert.True(PasswordSecurity.Verify(first, password));
        Assert.False(PasswordSecurity.Verify(first, RandomPassword()));
    }

    [Theory]
    [InlineData("")]
    [InlineData("RESET_REQUIRED")]
    [InlineData("PBKDF2-SHA256$999999999$AA==$AA==")]
    [InlineData("PBKDF2-SHA256$600000$not-base64$not-base64")]
    public void InvalidAndLegacyHashCannotAuthenticate(string hash) => Assert.False(PasswordSecurity.Verify(hash, RandomPassword()));

    [Fact]
    public void WeakAndOverlongPasswordsCannotBeCreated()
    {
        Assert.Throws<ArgumentException>(() => PasswordSecurity.Hash(""));
        Assert.Throws<ArgumentException>(() => PasswordSecurity.Hash(new string('x', 11)));
        Assert.Throws<ArgumentException>(() => PasswordSecurity.Hash(new string('x', 129)));
    }

    [Fact]
    public void HashIsNeverSerialized()
    {
        var user = new Usuario();
        user.EstablecerPassword(RandomPassword());
        var json = JsonSerializer.Serialize(user);
        Assert.DoesNotContain("PasswordHash", json);
        Assert.DoesNotContain(user.PasswordHash, json);
    }

    [Fact]
    public async Task AuditIdentityDoesNotCrossConcurrentRequests()
    {
        var security = ControladoraSeguridad.Instancia;
        var ready = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        int waiting = 0;
        async Task Check(int id)
        {
            using (security.IniciarContexto(new Usuario { Id = id }))
            {
                if (Interlocked.Increment(ref waiting) == 2) ready.SetResult();
                await ready.Task;
                Assert.Equal(id, security.UsuarioActual?.Id);
            }
            Assert.Null(security.UsuarioActual);
        }
        await Task.WhenAll(Task.Run(() => Check(100)), Task.Run(() => Check(200)));
        Assert.Null(security.UsuarioActual);
    }
}

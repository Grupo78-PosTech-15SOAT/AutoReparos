using AutoReparos.Application.Auth.Configurations;
using AutoReparos.Application.Auth.DTOs.Request;
using AutoReparos.Application.Auth.Services;
using AutoReparos.Domain.Clientes.ValueObjects;
using AutoReparos.Domain.Usuarios.Entities;
using AutoReparos.Domain.Usuarios.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace AutoReparos.Application.Tests
{
    public class AuthServiceTests
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly IOptions<JwtSettings> _jwtOptions;
        private readonly AuthService _authService;
        private readonly JwtSettings _settings;

        public AuthServiceTests()
        {
            _userManager = IdentityMockHelper.MockUserManager<Usuario>();
            _settings = new JwtSettings { Secret = "super_secret_key_with_enough_length_for_hmac256", ExpiryHours = 2 };
            _jwtOptions = Options.Create(_settings);
            _authService = new AuthService(_userManager, _jwtOptions);
        }

        [Fact(DisplayName = "Login With Valid Credentials Should Return Token")]
        public async Task Login_WithValidCredentials_ShouldReturnToken()
        {
            var request = new LoginRequestDTO("test@test.com", "Password123!");
            var user = new Usuario("Test User", Email.Create(request.Email), ETipoUsuario.Mecanico);

            _userManager.FindByEmailAsync(request.Email).Returns(user);
            _userManager.CheckPasswordAsync(user, request.Password).Returns(true);

            var result = await _authService.Login(request);

            result.Should().NotBeNull();
            result.Token.Should().NotBeNullOrEmpty();
            result.Email.Should().Be(user.Email);
            result.NomeCompleto.Should().Be(user.NomeCompleto);
        }

        [Fact(DisplayName = "Login With Invalid Email Should Return Null")]
        public async Task Login_WithInvalidEmail_ShouldReturnNull()
        {
            var request = new LoginRequestDTO("wrong@test.com", "Password123!");
            _userManager.FindByEmailAsync(request.Email).Returns((Usuario?)null);

            var result = await _authService.Login(request);

            result.Should().BeNull();
        }

        [Fact(DisplayName = "Login With Wrong Password Should Return Null")]
        public async Task Login_WithWrongPassword_ShouldReturnNull()
        {
            var request = new LoginRequestDTO("test@test.com", "WrongPassword!");
            var user = new Usuario("Test User", Email.Create(request.Email), ETipoUsuario.Mecanico);

            _userManager.FindByEmailAsync(request.Email).Returns(user);
            _userManager.CheckPasswordAsync(user, request.Password).Returns(false);

            var result = await _authService.Login(request);

            result.Should().BeNull();
        }
    }
}

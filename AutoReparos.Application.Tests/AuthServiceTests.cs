using AutoReparos.Application.Auth.Configurations;
using AutoReparos.Application.Auth.DTOs.Request;
using AutoReparos.Application.Auth.Services;
using AutoReparos.Domain.Usuarios.Entities;
using AutoReparos.Domain.Usuarios.Enums;
using AutoReparos.Domain.Usuarios.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace AutoReparos.Application.Tests
{
    public class AuthServiceTests
    {
        private readonly IAuthRepository _authRepository;
        private readonly IOptions<JwtSettings> _jwtOptions;
        private readonly AuthService _authService;
        private readonly JwtSettings _settings;

        public AuthServiceTests()
        {
            _authRepository = Substitute.For<IAuthRepository>();
            _settings = new JwtSettings { Secret = "super_secret_key_with_enough_length_for_hmac256", ExpiryHours = 2 };
            _jwtOptions = Options.Create(_settings);
            _authService = new AuthService(_authRepository, _jwtOptions);
        }

        [Fact(DisplayName = "Login With Valid Credentials Should Return Token")]
        public async Task Login_WithValidCredentials_ShouldReturnToken()
        {
            var request = new LoginRequestDTO("test@test.com", "Password123!");
            var usuario = new Usuario("Test Usuario", request.Email, ETipoUsuario.Mecanico);

            _authRepository.ValidateCredentialsAsync(request.Email, request.Password).Returns(usuario);

            var result = await _authService.Login(request);

            result.Should().NotBeNull();
            result.Token.Should().NotBeNullOrEmpty();
            result.Email.Should().Be(usuario.Email.Endereco);
            result.NomeCompleto.Should().Be(usuario.NomeCompleto);
        }

        [Fact(DisplayName = "Login With Invalid Email Should Return Null")]
        public async Task Login_WithInvalidEmail_ShouldReturnNull()
        {
            var request = new LoginRequestDTO("wrong@test.com", "Password123!");
            _authRepository.ValidateCredentialsAsync(request.Email, request.Password).Returns((Usuario?)null);

            var result = await _authService.Login(request);

            result.Should().BeNull();
        }

        [Fact(DisplayName = "Login With Wrong Password Should Return Null")]
        public async Task Login_WithWrongPassword_ShouldReturnNull()
        {
            var request = new LoginRequestDTO("test@test.com", "WrongPassword!");
            
            _authRepository.ValidateCredentialsAsync(request.Email, request.Password).Returns((Usuario?)null);

            var result = await _authService.Login(request);

            result.Should().BeNull();
        }
    }
}

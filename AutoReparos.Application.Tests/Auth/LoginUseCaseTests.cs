using AutoReparos.Application.Auth.DTOs.Request;
using AutoReparos.Application.Auth.Services.Interfaces;
using AutoReparos.Application.Auth.UseCases;
using AutoReparos.Domain.Usuarios.Entities;
using AutoReparos.Domain.Usuarios.Enums;
using AutoReparos.Domain.Usuarios.Repositories;
using FluentAssertions;
using NSubstitute;

namespace AutoReparos.Application.Tests.Auth
{
    public class LoginUseCaseTests
    {
        private readonly IAuthRepository _authRepository;
        private readonly IJwtService _jwtService;
        private readonly LoginUseCase _loginUseCase;

        public LoginUseCaseTests()
        {
            _authRepository = Substitute.For<IAuthRepository>();
            _jwtService = Substitute.For<IJwtService>();
            _loginUseCase = new LoginUseCase(_authRepository, _jwtService);
        }

        [Fact(DisplayName = "Login With Valid Credentials Should Return Token")]
        public async Task Login_WithValidCredentials_ShouldReturnToken()
        {
            var request = new LoginRequestDto("test@test.com", "Password123!");
            var usuario = new Usuario("Test Usuario", request.Email, ETipoUsuario.Mecanico);
            var expectedToken = "mocked-jwt-token";

            _authRepository.ValidateCredentialsAsync(request.Email, request.Password).Returns(usuario);
            _jwtService.GenerateToken(usuario).Returns(expectedToken);

            var result = await _loginUseCase.ExecuteAsync(request);

            result.Should().NotBeNull();
            result.Token.Should().Be(expectedToken);
            result.Email.Should().Be(usuario.Email.Endereco);
            result.NomeCompleto.Should().Be(usuario.NomeCompleto);
            result.Role.Should().Be(usuario.Tipo.ToString());
        }

        [Fact(DisplayName = "Login With Invalid Email Should Return Null")]
        public async Task Login_WithInvalidEmail_ShouldReturnNull()
        {
            var request = new LoginRequestDto("wrong@test.com", "Password123!");
            _authRepository.ValidateCredentialsAsync(request.Email, request.Password).Returns((Usuario?)null);

            var result = await _loginUseCase.ExecuteAsync(request);

            result.Should().BeNull();
        }

        [Fact(DisplayName = "Login With Wrong Password Should Return Null")]
        public async Task Login_WithWrongPassword_ShouldReturnNull()
        {
            var request = new LoginRequestDto("test@test.com", "WrongPassword!");

            _authRepository.ValidateCredentialsAsync(request.Email, request.Password).Returns((Usuario?)null);

            var result = await _loginUseCase.ExecuteAsync(request);

            result.Should().BeNull();
        }
    }
}

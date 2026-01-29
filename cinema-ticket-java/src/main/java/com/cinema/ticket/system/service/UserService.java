package com.cinema.ticket.system.service;

import com.cinema.ticket.system.dto.*;
import com.cinema.ticket.system.model.User;

import java.util.List;

public interface UserService {
    AuthResponse register(RegisterRequest request);
    AuthResponse login(LoginRequest request);
    AuthResponse refreshToken(String token);
    List<UserDTO> getAllUsers();
    UserDTO getUserById(Long id);
    User getUserEntityById(Long id);
    UserDTO updateUser(Long id, UpdateUserRequest request);
    void deleteUser(Long id);
    UserDTO getUserByEmail(String email);
}

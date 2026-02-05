package com.cinema.ticketsystem.mapper;

import com.cinema.ticketsystem.dto.UserDTO;
import com.cinema.ticketsystem.model.User;
import org.springframework.stereotype.Component;

@Component
public class UserMapper {
    
    public UserDTO toDTO(User user) {
        if (user == null) {
            return null;
        }
        
        UserDTO dto = new UserDTO();
        dto.setId(user.getId());
        dto.setEmail(user.getEmail());
        dto.setFirstName(user.getFirstName());
        dto.setLastName(user.getLastName());
        dto.setPhoneNumber(user.getPhoneNumber());
        dto.setIsAdmin(user.getIsAdmin());
        dto.setVersion(user.getVersion());
        return dto;
    }
}

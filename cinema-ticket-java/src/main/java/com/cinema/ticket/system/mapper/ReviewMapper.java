package com.cinema.ticket.system.mapper;

import com.cinema.ticket.system.dto.ReviewDTO;
import com.cinema.ticket.system.model.Review;
import org.springframework.stereotype.Component;

@Component
public class ReviewMapper {
    
    public ReviewDTO toDTO(Review review) {
        if (review == null) {
            return null;
        }
        
        ReviewDTO dto = new ReviewDTO();
        dto.setId(review.getId());
        dto.setUserId(review.getUser().getId());
        dto.setUserFullName(review.getUser().getFirstName() + " " + review.getUser().getLastName());
        dto.setMovieId(review.getMovie().getId());
        dto.setMovieTitle(review.getMovie().getTitle());
        dto.setRating(review.getRating());
        dto.setComment(review.getComment());
        dto.setCreatedAt(review.getCreatedAt());
        dto.setUpdatedAt(review.getUpdatedAt());
        return dto;
    }
}

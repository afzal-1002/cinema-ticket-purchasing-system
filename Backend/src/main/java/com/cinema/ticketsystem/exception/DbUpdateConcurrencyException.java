package com.cinema.ticketsystem.exception;

/**
 * Application-level exception used to signal optimistic locking conflicts while updating or deleting entities.
 */
public class DbUpdateConcurrencyException extends RuntimeException {
    public DbUpdateConcurrencyException(String message) {
        super(message);
    }

    public DbUpdateConcurrencyException(String message, Throwable cause) {
        super(message, cause);
    }
}

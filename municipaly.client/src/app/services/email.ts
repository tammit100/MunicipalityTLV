import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

// הגדרת הממשק (Interface) של התשובה מהשרת
export interface EmailResponse {
  email: string;
  receivedAt: string;
}

@Injectable({
  providedIn: 'root' // הופך את ה-Service לזמין בכל האפליקציה
})
export class EmailService {
  private readonly apiUrl = 'https://localhost:7189/api/email'; // שנה לפורט שלך

  constructor(private http: HttpClient) {}

  sendEmail(email: string): Observable<EmailResponse> {
    return this.http.post<EmailResponse>(this.apiUrl, { email });
  }
}
